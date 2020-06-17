using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a blob signature that can be referenced by metadata token. 
    /// </summary>
    /// <remarks>
    /// Stand-alone signatures are often used by the runtime for referencing local variable signatures, or serve
    /// as an operand for calli instructions.
    /// </remarks>
    public class StandAloneSignature : IHasCustomAttribute
    {
        private readonly LazyVariable<CallingConventionSignature> _signature;
        private IList<CustomAttribute> _customAttributes;
        private MetadataToken _token;

        /// <summary>
        /// Initializes a new stand-alone signature.
        /// </summary>
        /// <param name="token">The token of the stand-alone signature.</param>
        protected StandAloneSignature(MetadataToken token)
        {
            _token = token;
            _signature = new LazyVariable<CallingConventionSignature>(GetSignature);
        }
        
        /// <summary>
        /// Wraps a blob signature into a new stand-alone signature.
        /// </summary>
        /// <param name="signature">The signature to assign a metadata token.</param>
        public StandAloneSignature(CallingConventionSignature signature)
            : this(new MetadataToken(TableIndex.StandAloneSig, 0))
        {
            Signature = signature;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken => _token;

        MetadataToken IMetadataMember.MetadataToken
        {
            get => _token;
            set => _token = value;
        }

        /// <summary>
        /// Gets or sets the signature that was referenced by this metadata member.
        /// </summary>
        public CallingConventionSignature Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get
            {
                if (_customAttributes is null)
                    Interlocked.CompareExchange(ref _customAttributes, GetCustomAttributes(), null);
                return _customAttributes;
            }
        }

        /// <summary>
        /// Obtains the signature referenced by this metadata member.
        /// </summary>
        /// <returns>The signature</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual CallingConventionSignature GetSignature() => null;

        /// <summary>
        /// Obtains the list of custom attributes assigned to the member.
        /// </summary>
        /// <returns>The attributes</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CustomAttributes"/> property.
        /// </remarks>
        protected virtual IList<CustomAttribute> GetCustomAttributes() =>
            new OwnedCollection<IHasCustomAttribute, CustomAttribute>(this);

        /// <inheritdoc />
        public override string ToString() => Signature.ToString();
    }
}