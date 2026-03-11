// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Settings.Config;

/// <summary>
/// Represents the CORS configuration for the generated API.
/// </summary>
/// <param name="Origins">The allowed origins for CORS requests.</param>
/// <param name="Methods">The allowed HTTP methods for CORS requests.</param>
/// <param name="Headers">The allowed headers for CORS requests.</param>
public record NinjadogCorsConfiguration(
    string[] Origins,
    string[]? Methods = null,
    string[]? Headers = null);
