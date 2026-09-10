using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Principal;
using PermissionScope.Core;

namespace PermissionScope.Windows;

public static class DirectoryGroupResolver
{
    public static DirectoryIdentity? Resolve(Identity identity, GroupGraph graph, List<string> warnings, CancellationToken cancellation)
    {
        var split = identity.Name.Split('\\', 2);
        if (split.Length != 2 || split[0].Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase) || split[0] is "BUILTIN" or "NT AUTHORITY") return null;
        try
        {
            cancellation.ThrowIfCancellationRequested();
            using var ldap = new LdapConnection(new LdapDirectoryIdentifier(split[0], 389), CredentialCache.DefaultNetworkCredentials, AuthType.Negotiate);
            ldap.Timeout = TimeSpan.FromSeconds(4); ldap.SessionOptions.ProtocolVersion = 3;
            ldap.SessionOptions.Signing = true; ldap.SessionOptions.Sealing = true; ldap.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
            ldap.Bind();
            SearchResultEntry? Search(string dn, string filter, SearchScope scope, params string[] attributes)
            {
                cancellation.ThrowIfCancellationRequested();
                var request = new SearchRequest(dn, filter, scope, attributes) { TimeLimit = TimeSpan.FromSeconds(4), SizeLimit = 2 };
                var response = (SearchResponse)ldap.SendRequest(request);
                return response.Entries.Count == 1 ? response.Entries[0] : null;
            }
            var root = Search("", "(objectClass=*)", SearchScope.Base, "defaultNamingContext");
            var namingContext = root?.Attributes["defaultNamingContext"]?[0]?.ToString();
            if (namingContext == null) { warnings.Add("LDAP returned no default naming context."); return null; }
            var sid = new SecurityIdentifier(identity.Sid); var bytes = new byte[sid.BinaryLength]; sid.GetBinaryForm(bytes, 0);
            var encodedSid = string.Concat(bytes.Select(b => "\\" + b.ToString("X2")));
            var user = Search(namingContext, "(objectSid=" + encodedSid + ")", SearchScope.Subtree, "objectSid", "memberOf", "primaryGroupID", "userAccountControl", "sIDHistory");
            if (user == null) { warnings.Add("LDAP identity was not uniquely resolved in " + split[0] + "."); return null; }
            var source = "LDAP " + split[0];
            var history = user.Attributes["sIDHistory"]?.GetValues(typeof(byte[])).Cast<byte[]>().Select(value => new SecurityIdentifier(value, 0).Value).ToArray() ?? [];
            var accountControl = int.TryParse(user.Attributes["userAccountControl"]?[0]?.ToString(), out var flags) ? flags : 0;
            var primary = user.Attributes["primaryGroupID"]?[0]?.ToString();
            var primarySid = primary != null && sid.AccountDomainSid != null ? sid.AccountDomainSid.Value + "-" + primary : null;
            if (primarySid != null) graph.Add(new(identity.Sid, primarySid, source + " / primaryGroupID / " + user.DistinguishedName));
            var queue = new Queue<(string Member, string ParentDn)>();
            void Enqueue(string member, SearchResultEntry entry)
            {
                if (entry.Attributes["memberOf"] is { } parents)
                    foreach (var value in parents.GetValues(typeof(string)).Cast<string>()) queue.Enqueue((member, value));
                if (entry.Attributes.AttributeNames.Cast<string>().Any(a => a.StartsWith("memberOf;range=", StringComparison.OrdinalIgnoreCase))) warnings.Add("LDAP membership range requires further retrieval: " + entry.DistinguishedName);
            }
            Enqueue(identity.Sid, user);
            var cache = new Dictionary<string, SearchResultEntry?>(StringComparer.OrdinalIgnoreCase);
            var expanded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (queue.TryDequeue(out var step))
            {
                cancellation.ThrowIfCancellationRequested();
                if (cache.Count >= 10000) { warnings.Add("LDAP graph safety bound reached; graph is incomplete."); break; }
                if (!cache.TryGetValue(step.ParentDn, out var parent)) cache[step.ParentDn] = parent = Search(step.ParentDn, "(objectClass=group)", SearchScope.Base, "objectSid", "memberOf");
                if (parent?.Attributes["objectSid"]?[0] is not byte[] groupBytes) { warnings.Add("Unresolved LDAP group: " + step.ParentDn); continue; }
                var groupSid = new SecurityIdentifier(groupBytes, 0).Value;
                graph.Add(new(step.Member, groupSid, source + " / memberOf / " + step.ParentDn));
                if (expanded.Add(groupSid)) Enqueue(groupSid, parent);
            }
            return new(identity.Sid, user.DistinguishedName, (accountControl & 2) != 0, primarySid, history, source);
        }
        catch (Exception error) when (error is LdapException or DirectoryOperationException or System.ComponentModel.Win32Exception or ArgumentException)
        { warnings.Add("Directory membership could not be fully resolved: " + error.Message); return null; }
    }
}
