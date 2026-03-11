// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Settings.Validation;

/// <summary>
/// Represents the severity level of a validation diagnostic.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>A validation error that prevents the configuration from being used.</summary>
    Error,

    /// <summary>A validation warning that indicates a potential issue.</summary>
    Warning,

    /// <summary>An informational validation message.</summary>
    Info
}
