// // @file LocalizationTestContext.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Cultures;

namespace RetroEngine.Portable.Test;

[SetUpFixture]
public class LocalizationTestContext
{
    [OneTimeSetUp]
    public void SetupLocalization()
    {
        _ = CultureManager.Instance;
    }

    [OneTimeTearDown]
    public void TearDownLocalization()
    {
        CultureManager.Instance.Dispose();
    }
}
