﻿using System;

namespace Ddon.Core.Use.Cronos;

/// <summary>
/// Defines the cron format options that customize string parsing for <see cref="CronExpression.Parse(string, CronFormat)"/>.
/// </summary>
[Flags]
public enum CronFormat
{
    /// <summary>
    /// Parsing string must contain only 5 fields: minute, hour, day of month, month, day of week.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Second field must be specified in parsing string.
    /// </summary>
    IncludeSeconds = 1
}
