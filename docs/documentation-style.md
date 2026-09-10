# Documentation maintenance

Lead with the task and show one concrete result. Introduce the plain-language meaning before API names and masks. Keep evidence and uncertainty visible together. Use Unknown consistently; never rewrite an unverified result as denied or allowed.

The English README and seven translations are generated from the same field schema. Localized walkthroughs use matching native captures. Technical references remain English and are linked explicitly. Translation previews are not described as independently reviewed.

Keep screenshot explanations in nearby text and alt text. Do not paint substitute UI or anonymization over a live customer image: regenerate from `DemoFixture` instead. Scroll positions are set through the app's own accessibility controls. The raw captures remain unedited. An illustration must be labeled as an illustration, not an app capture.

For any behavior change, use the impact matrix in [development](development.md). Verify generated text, links, image hashes, fixture privacy and screenshot source fingerprints before merging. Review the actual images in at least English, Portuguese and RTL, plus any locale whose text/layout changed. Preserve exact paths, API names and CLI flags in translated technical examples.

Do not claim domain completeness, certifications, comparative superiority, benchmark scale or distribution availability that has not been demonstrated. Public limitations belong in release scope, not hidden in an issue thread.
