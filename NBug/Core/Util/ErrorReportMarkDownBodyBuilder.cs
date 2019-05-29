 // --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitHub.cs" company="Git Extensions">
//   Copyright (c) 2019 Igor Velikorossov. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text;

namespace NBug.Core.Util
{
    public interface IErrorReportMarkDownBodyBuilder
    {
        string Build(Exception exception, string additionalInfo);
    }

    public sealed class ErrorReportMarkDownBodyBuilder : IErrorReportMarkDownBodyBuilder
    {
        public string Build(Exception exception, string additionalInfo)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var sb = new StringBuilder();

            sb.AppendLine(@"<!--
    :warning: Review existing issues to see whether someone else has already reported your issue.
-->

The sections below must be filled in and this text must be removed or the issue will be closed.

## Current behaviour

<!-- Be as specific and detailed as possible to help us identify your issue. -->


## Expected behaviour


## Steps to reproduce

<!-- Take some time to try and reproduce the issue, then explain how to do so here. -->


## Error Details");
            sb.AppendLine("```");
            sb.AppendLine(exception.ToString());
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(additionalInfo))
            {
                sb.AppendLine("## Additional information");
                sb.AppendLine(additionalInfo.Trim());
                sb.AppendLine();
                sb.AppendLine();
            }

            try
            {
                sb.AppendLine("## Environment");

                var systemInfo = Settings.GetSystemInfo?.Invoke();
                if (!string.IsNullOrWhiteSpace(systemInfo))
                {
                    sb.AppendLine(systemInfo);
                }
                else
                {
                    sb.AppendLine("System information is not supplied");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to retrieve system information.");
                sb.AppendLine("Exception:");
                sb.AppendLine("```");
                sb.AppendLine(ex.ToString());
                sb.AppendLine("```");
            }

            return sb.ToString();
        }
    }
}
