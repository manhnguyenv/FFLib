using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NSubstitute;

namespace FFLibUnitTests.Data
{
    [TestFixture]
    class DBTableTests
    {
        [Test]
        public void Load()
        {
            //Initialize
            FFLib.Data.DBProviders.IDBProvider dbprovider = Substitute.For<FFLib.Data.DBProviders.IDBProvider>();
            FFLib.Data.IDBConnection Conn = Substitute.For<FFLib.Data.IDBConnection>();
            FFLib.Data.DBTable<TestDTO> dtoTable;
            TestDTO expected = new TestDTO();
            expected.ID = 1;

            //Setup
            System.Data.DataTable dr = new System.Data.DataTable();
            dr.Columns.Add("ID", typeof(int));
            dr.Columns.Add("Name", typeof(String));
            dr.Columns.Add("Status", typeof(string));
            dr.Columns.Add("CreateDate", typeof(DateTime));
            System.Data.DataRow drRow = dr.NewRow();
            drRow["ID"] = 1;
            drRow["Name"] = "test";
            drRow["Status"] = "open";
            drRow["CreateDate"] = DateTime.Now;
            dr.Rows.Add(drRow);

            System.Data.IDataReader idr = dr.CreateDataReader();

            Conn.InTrx.Returns<bool>(false);
            Conn.dbProvider.Returns<FFLib.Data.DBProviders.IDBProvider>(dbprovider);
            Conn.State.Returns<System.Data.ConnectionState>(System.Data.ConnectionState.Closed);

            dbprovider.ExecuteReader(null, null, null).ReturnsForAnyArgs(idr);

            //Execute
            dtoTable = new FFLib.Data.DBTable<TestDTO>(Conn);
            TestDTO result = dtoTable.Load(1);
            Assert.AreEqual(expected.ID, result.ID);


        }

        [Test]
        public void LoadWithFieldMap()
        {
            //Initialize
            FFLib.Data.DBProviders.IDBProvider dbprovider = Substitute.For<FFLib.Data.DBProviders.IDBProvider>();
            FFLib.Data.IDBConnection Conn = Substitute.For<FFLib.Data.IDBConnection>();
            FFLib.Data.DBTable<TestDTO2> dtoTable;
            TestDTO2 expected = new TestDTO2();
            expected.XID = 1;

            //Setup
            System.Data.DataTable dr = new System.Data.DataTable();
            dr.Columns.Add("ID", typeof(int));
            dr.Columns.Add("Name", typeof(String));
            dr.Columns.Add("Status", typeof(string));
            dr.Columns.Add("CreateDate", typeof(DateTime));
            System.Data.DataRow drRow = dr.NewRow();
            drRow["ID"] = 1;
            drRow["Name"] = "test";
            drRow["Status"] = "open";
            drRow["CreateDate"] = DateTime.Now;
            dr.Rows.Add(drRow);

            System.Data.IDataReader idr = dr.CreateDataReader();

            Conn.InTrx.Returns<bool>(false);
            Conn.dbProvider.Returns<FFLib.Data.DBProviders.IDBProvider>(dbprovider);
            Conn.State.Returns<System.Data.ConnectionState>(System.Data.ConnectionState.Closed);

            dbprovider.ExecuteReader(null,null,null).ReturnsForAnyArgs(idr);

            //Execute
            dtoTable = new FFLib.Data.DBTable<TestDTO2>(Conn);
            TestDTO2 result = dtoTable.Load(1);
            Assert.AreEqual(expected.XID, result.XID);
           
        }

        [Test]
        public void Save()
        {
            //Initialize
            FFLib.Data.DBProviders.IDBProvider dbprovider = Substitute.For<FFLib.Data.DBProviders.IDBProvider>();
            FFLib.Data.IDBConnection Conn = Substitute.For<FFLib.Data.IDBConnection>();
            FFLib.Data.DBTable<TestDTO> dtoTable;
            TestDTO testData = new TestDTO();
            testData.ID = 1;
            testData.Name = "Test";
            testData.Status = "Open";
            testData.CreateDate = DateTime.Today;

            //Setup
            dbprovider.DBUpdate(NSubstitute.Arg.Any<FFLib.Data.IDBConnection>()
                , NSubstitute.Arg.Is<string>("UPDATE #__TableName SET \n[Locked] = @p1,[ID] = @p2,[Name] = @p3,[Status] = @p4,[CreateDate] = @p5\n WHERE [ID] = @pk")
                , NSubstitute.Arg.Any<FFLib.Data.SqlParameter[]>());
            Conn.InTrx.Returns<bool>(false);
            Conn.dbProvider.Returns<FFLib.Data.DBProviders.IDBProvider>(dbprovider);
            Conn.State.Returns<System.Data.ConnectionState>(System.Data.ConnectionState.Closed);

            //Execute
            dtoTable = new FFLib.Data.DBTable<TestDTO>(Conn);
            dtoTable.Save(testData);

        }

        [Test]
        public void SaveWithFieldMap()
        {
            //Initialize
            FFLib.Data.DBProviders.IDBProvider dbprovider = Substitute.For<FFLib.Data.DBProviders.IDBProvider>();
            FFLib.Data.IDBConnection Conn = Substitute.For<FFLib.Data.IDBConnection>();
            FFLib.Data.DBTable<TestDTO2> dtoTable;
            TestDTO2 testData = new TestDTO2();
            testData.XID = 1;
            testData.Name = "Test";
            testData.Status = "Open";
            testData.CreateDate = DateTime.Today;

            //Setup
            dbprovider.DBUpdate(NSubstitute.Arg.Any<FFLib.Data.IDBConnection>()
                , NSubstitute.Arg.Is<string>("UPDATE #__TableName SET \n[Locked] = @p1,[ID] = @p2,[Name] = @p3,[Status] = @p4,[CreateDate] = @p5\n WHERE [ID] = @pk")
                , NSubstitute.Arg.Any<FFLib.Data.SqlParameter[]>());
            Conn.InTrx.Returns<bool>(false);
            Conn.dbProvider.Returns<FFLib.Data.DBProviders.IDBProvider>(dbprovider);
            Conn.State.Returns<System.Data.ConnectionState>(System.Data.ConnectionState.Closed);

            //Execute
            dtoTable = new FFLib.Data.DBTable<TestDTO2>(Conn);
            dtoTable.Save(testData);


            //Assert that all the fields exist in the SQL parameters some how.
        }
    }
class TestDTO
    {
        [FFLib.Data.Attributes.PrimaryKey]
        public int ID;
        public string Name;
        public string Status;
        public DateTime CreateDate;
        public bool IsLocked { get; set; }
    }

    class TestDTO2
    {
        [FFLib.Attributes.MapsTo("ID")]
        [FFLib.Data.Attributes.PrimaryKey]
        public int XID;
        public string Name;
        public string Status;
        public DateTime CreateDate;
        [FFLib.Attributes.MapsTo("Locked")]
        public bool IsLocked { get; set; }
    }
}