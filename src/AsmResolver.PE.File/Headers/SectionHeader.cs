// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Text;

namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Represents a single section header in the portable executable (PE) file format.
    /// </summary>
    public class SectionHeader : ISegment, IOffsetConverter
    {
        public const uint SectionHeaderSize = 8 * sizeof(byte) +
                                             6 * sizeof (uint) +
                                             2 * sizeof (ushort) +
                                             1 * sizeof (uint);

        private uint _fileOffset;
        private string _name;

        /// <summary>
        /// Reads a single section header at the current position of the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The section header that was read.</returns>
        public static SectionHeader FromReader(IBinaryStreamReader reader)
        {
            uint offset = reader.FileOffset;

            var nameBytes = new byte[8];
            reader.ReadBytes(nameBytes, 0, nameBytes.Length);

            return new SectionHeader(Encoding.UTF8.GetString(nameBytes).Replace("\0", ""), 0)
            {
                _fileOffset = offset,
                VirtualSize = reader.ReadUInt32(),
                VirtualAddress = reader.ReadUInt32(),
                SizeOfRawData = reader.ReadUInt32(),
                PointerToRawData = reader.ReadUInt32(),
                PointerToRelocations = reader.ReadUInt32(),
                PointerToLineNumbers = reader.ReadUInt32(),
                NumberOfRelocations = reader.ReadUInt16(),
                NumberOfLineNumbers = reader.ReadUInt16(),
                Characteristics = (SectionFlags) reader.ReadUInt32()
            };
        }

        /// <summary>
        /// Creates a new section header with the provided name.
        /// </summary>
        /// <param name="name">The name of the new section.</param>
        /// <param name="characteristics">The section flags to assign.</param>
        public SectionHeader(string name, SectionFlags characteristics)
        {
            Name = name;
            Characteristics = characteristics;
        }

        public SectionHeader(SectionHeader value)
        {
            _fileOffset = value._fileOffset;
            VirtualSize = value.VirtualSize;
            VirtualAddress = value.VirtualAddress;
            SizeOfRawData = value.SizeOfRawData;
            PointerToRawData = value.PointerToRawData;
            PointerToRelocations = value.PointerToRelocations;
            PointerToLineNumbers = value.PointerToLineNumbers;
            NumberOfRelocations = value.NumberOfRelocations;
            NumberOfLineNumbers = value.NumberOfLineNumbers;
            Characteristics = value.Characteristics;
        }


        /// <inheritdoc />
        uint IOffsetProvider.FileOffset => _fileOffset;

        /// <inheritdoc />
        uint IOffsetProvider.Rva => _fileOffset;

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        /// <remarks>
        /// The name of the section is a UTF-8 string that can be no longer than 8 characters long.
        /// </remarks>
        public string Name
        {
            get => _name;
            set
            {
                if (Encoding.UTF8.GetByteCount(value) > 8)
                    throw new ArgumentException("Name is too long.");
                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the total number of bytes the section consists of when it is loaded into memory.
        /// </summary>
        /// <remarks>
        /// If the value is greater than <see cref="SizeOfRawData"/>, it is zero-padded when loaded into memory.
        /// </remarks>
        public uint VirtualSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the virtual address (relative to the image base) of the section.
        /// </summary>
        public uint VirtualAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of bytes the section consists of on disk.
        /// </summary>
        public uint SizeOfRawData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file offset to the beginning of the section on disk.
        /// </summary>
        public uint PointerToRawData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file offset to the beginning of the relocations table for the section. 
        /// </summary>
        /// <remarks>
        /// This field is set to zero in a normal PE image. 
        /// </remarks>
        public uint PointerToRelocations
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file offset to the beginning of the line numbers table for the section. 
        /// </summary>
        /// <remarks>
        /// This field is set to zero in a normal PE image.
        /// </remarks>
        public uint PointerToLineNumbers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of relocations in the relocation table of the section.
        /// </summary>
        /// <remarks>
        /// This field is set to zero for executable files. 
        /// </remarks>
        public ushort NumberOfRelocations
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total amount of line numbers table referenced by <see cref="PointerToLineNumbers" /> for the section. 
        /// </summary>
        /// <remarks>
        /// This field is not used in a normal PE image.
        /// </remarks>
        public ushort NumberOfLineNumbers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the characteristics of the section.
        /// </summary>
        public SectionFlags Characteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the section contains executable code.
        /// </summary>
        public bool IsContentCode
        {
            get => (Characteristics & SectionFlags.ContentCode) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.ContentCode)
                                     | (value ? SectionFlags.ContentCode : 0);
        }
        /// <summary>
        /// Gets or sets a value indicating the section contains initialized data.
        /// </summary>
        public bool IsContentInitializedData
        {
            get => (Characteristics & SectionFlags.ContentInitializedData) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.ContentInitializedData)
                                     | (value ? SectionFlags.ContentInitializedData : 0);
        }
        /// <summary>
        /// Gets or sets a value indicating the section contains uninitialized data.
        /// </summary>
        public bool IsContentUninitializedData
        {
            get => (Characteristics & SectionFlags.ContentUninitializedData) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.ContentUninitializedData)
                                     | (value ? SectionFlags.ContentUninitializedData : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section can be discarded as needed.
        /// </summary>
        public bool IsMemoryDiscardable
        {
            get => (Characteristics & SectionFlags.MemoryDiscardable) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryDiscardable)
                                     | (value ? SectionFlags.MemoryDiscardable : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section cannot be cached.
        /// </summary>
        public bool IsMemoryNotCached
        {
            get => (Characteristics & SectionFlags.MemoryNotCached) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryNotCached)
                                     | (value ? SectionFlags.MemoryNotCached : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section is not pageable.
        /// </summary>
        public bool IsMemoryNotPaged
        {
            get => (Characteristics & SectionFlags.MemoryNotPaged) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryNotPaged)
                                     | (value ? SectionFlags.MemoryNotPaged : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section can be shared in memory.
        /// </summary>
        public bool IsMemoryShared
        {
            get => (Characteristics & SectionFlags.MemoryShared) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryShared)
                                     | (value ? SectionFlags.MemoryShared : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section can be executed as code.
        /// </summary>
        public bool IsMemoryExecute
        {
            get => (Characteristics & SectionFlags.MemoryExecute) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryExecute)
                                     | (value ? SectionFlags.MemoryExecute : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section can be read.
        /// </summary>
        public bool IsMemoryRead
        {
            get => (Characteristics & SectionFlags.MemoryRead) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryRead)
                                     | (value ? SectionFlags.MemoryRead : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the section can be written.
        /// </summary>
        public bool IsMemoryWrite
        {
            get => (Characteristics & SectionFlags.MemoryWrite) != 0;
            set => Characteristics = (Characteristics & ~SectionFlags.MemoryWrite)
                                     | (value ? SectionFlags.MemoryWrite : 0);
        }

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            _fileOffset = newFileOffset;
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            return SectionHeaderSize;
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return GetPhysicalSize();
        }

        /// <summary>
        /// Determines whether the provided file offset falls within the section that the header describes. 
        /// </summary>
        /// <param name="fileOffset">The offset to check.</param>
        /// <returns><c>true</c> if the file offset falls within the section, <c>false</c> otherwise.</returns>
        public bool ContainsFileOffset(uint fileOffset)
        {
            return PointerToRawData <= fileOffset && fileOffset < PointerToRawData + SizeOfRawData;
        }

        /// <summary>
        /// Determines whether the provided virtual address falls within the section that the header describes. 
        /// </summary>
        /// <param name="rva">The virtual address to check.</param>
        /// <returns><c>true</c> if the virtual address falls within the section, <c>false</c> otherwise.</returns>
        public bool ContainsRva(uint rva)
        {
            return VirtualAddress <= rva && rva < VirtualAddress + VirtualSize;
        }

        /// <inheritdoc />
        public uint FileOffsetToRva(uint fileOffset)
        {
            if (!ContainsFileOffset(fileOffset))
                throw new ArgumentOutOfRangeException(nameof(fileOffset));
            return fileOffset - PointerToRawData + VirtualAddress;
        }

        /// <inheritdoc />
        public uint RvaToFileOffset(uint rva)
        {
            if (!ContainsRva(rva))
                throw new ArgumentOutOfRangeException(nameof(rva));
            return rva - VirtualAddress + PointerToRawData;
        }


        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            var nameBytes = Encoding.UTF8.GetBytes(Name ?? string.Empty);
            writer.WriteBytes(nameBytes);
            writer.WriteZeroes(8 - nameBytes.Length);

            writer.WriteUInt32(VirtualSize);
            writer.WriteUInt32(VirtualAddress);
            writer.WriteUInt32(SizeOfRawData);
            writer.WriteUInt32(PointerToRawData);
            writer.WriteUInt32(PointerToRelocations);
            writer.WriteUInt32(PointerToLineNumbers);
            writer.WriteUInt16(NumberOfRelocations);
            writer.WriteUInt16(NumberOfLineNumbers);
            writer.WriteUInt32((uint) Characteristics);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} ({nameof(VirtualAddress)}: {VirtualAddress:X8}, " +
                   $"{nameof(VirtualSize)}: {VirtualSize:X8}, " +
                   $"{nameof(PointerToRawData)}: {PointerToRawData:X8}, " +
                   $"{nameof(SizeOfRawData)}: {SizeOfRawData:X8}, " +
                   $"{nameof(Characteristics)}: {Characteristics})";
        }

    }
}

