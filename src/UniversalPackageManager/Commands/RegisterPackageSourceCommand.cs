// Copyright (c) Thomas Nieto - All Rights Reserved
// You may use, distribute and modify this code under the
// terms of the MIT license.

using System;
using System.Linq;
using System.Management.Automation;
using UniversalPackageManager.Commands.Internal;
using UniversalPackageManager.Provider;

namespace UniversalPackageManager.Commands
{
    [Cmdlet(VerbsLifecycle.Register, "PackageSource", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low)]
    [OutputType(typeof(PackageSourceInfo))]
    public sealed class RegisterPackageSourceCommand : SourceCommandBase
    {
        private const PackageProviderOperations SetSource = PackageProviderOperations.SetSource;

        /// <summary>
        /// Gets or sets the source name.
        /// </summary>
        [Parameter(Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source location.
        /// </summary>
        /// <value></value>
        [Parameter(Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        [Parameter(Mandatory = true,
            Position = 2)]
        [ValidateNotNullOrEmpty]
        [ValidateProvider(SetSource)]
        [ArgumentCompleter(typeof(ProviderArgumentCompleter))]
        public override string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets if the source is trusted.
        /// </summary>
        [Parameter]
        public SwitchParameter Trusted { get; set; }

        /// <summary>
        /// Gets or sets if the command should pass objects through.
        /// </summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets if the source should be overwritten.
        /// </summary>
        [Parameter]
        public SwitchParameter Force { get; set; }

        public RegisterPackageSourceCommand()
        {
            Operation = SetSource;
        }

        protected override void ProcessRecord()
        {
            if (!ShouldProcess(Name))
            {
                return;
            }

            var instance = GetInstances(Provider).First();

            WriteVerbose($"Registering '{Name}' source.");
            SetRequest(Name, Location, Trusted, Force);

            WriteVerbose($"Calling '{instance.ProviderInfo.Name}' provider.");
            Request.ProviderInfo = instance.ProviderInfo;

            try
            {
                instance.RegisterSource(Request);
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception e)
            {
                var ex = new PackageProviderException(e.Message, e);
                var er = new ErrorRecord(ex, "PackageProviderError", ErrorCategory.NotSpecified, Name);
                WriteError(er);
            }

            if (!Request.HasWriteObject)
            {
                var ex = new PackageProviderException("Package provider did not register package source.");
                var err = new ErrorRecord(ex, "PackageSourceNotRegistered", ErrorCategory.NotSpecified, Name);
                WriteError(err);
            }
        }

        protected override void SetRequest()
        {
            base.SetRequest();
            Request.PassThru = PassThru;
        }
    }
}