// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Settings.Validation;

/// <summary>
/// Represents a single validation diagnostic containing a code, message, severity, and JSON path.
/// </summary>
/// <param name="Code">The diagnostic code (e.g., NINJ001).</param>
/// <param name="Message">A human-readable description of the issue.</param>
/// <param name="Severity">The severity level of the diagnostic.</param>
/// <param name="Path">The JSON path where the issue was found.</param>
public sealed record ValidationDiagnostic(
    string Code,
    string Message,
    ValidationSeverity Severity,
    string Path);
