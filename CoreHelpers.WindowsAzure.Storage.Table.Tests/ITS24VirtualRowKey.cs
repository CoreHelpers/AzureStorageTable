using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Storable(Tablename = "VirtualRowKeyNone")]
    [VirtualRowKey("{{RK}}")]
    public class VirtualRowKeyNone
    {
        [PartitionKey]
        public string PK { get; set; } = String.Empty;
        
        public string RK { get; set; } = String.Empty;        
    }

    [Storable(Tablename = "VirtualRowKeyNone")]
    [VirtualRowKey("{{RK}}-{{RK2}}")]
    public class VirtualRowKeyCombinedNone: VirtualRowKeyNone
    {
        public string RK2 { get; set; } = String.Empty;
    }

    [Storable(Tablename = "VirtualRowKeyNone")]
    [VirtualRowKey("{{RK}}", nVirtualValueEncoding.Base64)]
    public class VirtualRowKeyBase64 : VirtualRowKeyNone
    {}

    [Storable(Tablename = "VirtualRowKeyNone")]
    [VirtualRowKey("{{RK}}", nVirtualValueEncoding.Sha256)]
    public class VirtualRowKeySha256: VirtualRowKeyNone
    { }

    public class ITS24VirtualRowKey
    {
        private readonly IStorageContext _rootContext;

        public ITS24VirtualRowKey(IStorageContext context)
        {
            _rootContext = context;
        }

        [Fact]
        public void VerifyVirtualRowKeyNoneEncoding()
        {
            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();

                // configure the entity mapper
                scp.AddAttributeMapper<VirtualRowKeyNone>();

                // check the entity
                var entity = TableEntityDynamic.ToEntity<VirtualRowKeyNone>(new VirtualRowKeyNone() { PK = "P01", RK = "R01" }, scp);
                Assert.Equal("R01", entity.RowKey);
            }
        }

        [Fact]
        public void VerifyVirtualRowKeyNoneEncodingCombined()
        {
            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();

                // configure the entity mapper
                scp.AddAttributeMapper<VirtualRowKeyCombinedNone>();

                // check the entity
                var entity = TableEntityDynamic.ToEntity<VirtualRowKeyCombinedNone>(new VirtualRowKeyCombinedNone() { PK = "P01", RK = "R01", RK2 = "CMB" }, scp);
                Assert.Equal("R01-CMB", entity.RowKey);
            }
        }

        [Fact]
        public void VerifyVirtualRowKeyBase64Encoding()
        {
            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();

                // configure the entity mapper
                scp.AddAttributeMapper<VirtualRowKeyBase64>();

                // check the entity
                var entity = TableEntityDynamic.ToEntity<VirtualRowKeyBase64>(new VirtualRowKeyBase64() { PK = "P01", RK = "R01" }, scp);
                Assert.Equal("UjAx", entity.RowKey);
            }
        }

        [Fact]
        public void VerifyVirtualRowKeySha256Encoding()
        {
            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();

                // configure the entity mapper
                scp.AddAttributeMapper<VirtualRowKeySha256>();

                // check the entity
                var entity = TableEntityDynamic.ToEntity<VirtualRowKeySha256>(new VirtualRowKeySha256() { PK = "P01", RK = "R01" }, scp);
                Assert.Equal("e0a64b0b6d837fa4edc328ab9ddea0e3e7e0e4f715304c1d6bf3d0adc9d5292a", entity.RowKey);
            }
        }
    }
}