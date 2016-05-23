namespace EdFi.Common.Services
{
    public interface IHostedService
    {
        void Start();
        void Stop();

        /// <summary>
        /// Provides the description to be displayed in Windows Services Console or other management programs
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Provides the name to be displayed in Windows Services Console or other managemenet programs
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Provides the name to be shown in the Windows Services Console as the Service Name - may be the same or different from display name
        /// </summary>
        string ServiceName { get; }
    }
}