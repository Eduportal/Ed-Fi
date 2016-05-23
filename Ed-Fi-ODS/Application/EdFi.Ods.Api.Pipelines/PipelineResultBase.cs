using System;

namespace EdFi.Ods.Pipelines
{
    public abstract class PipelineResultBase
    {
        /// <summary>
        /// Gets or sets the exception that occurred during pipeline processing.
        /// </summary>
        public Exception Exception { get; set; }
    }
}