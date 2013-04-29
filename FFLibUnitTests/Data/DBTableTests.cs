using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino;
using Rhino.Mocks;

namespace FFLibUnitTests.Data
{
    [TestFixture]
    class DBTableTests
    {
        [Test]
        public void Load()
        {
            MockRepository mocks = new MockRepository(); 
            FFLib.Data.DBProviders.IDBProvider dbprovider = mocks.Stub<FFLib.Data.DBProviders.IDBProvider>();
            FFLib.Data.IDBConnection Conn = mocks.Stub<FFLib.Data.IDBConnection>();
            FFLib.Data.DBTable<TestDTO> dtoTable;
            TestDTO expected = new TestDTO();
            expected.ID = 1;


            System.Data.DataTable dr = new System.Data.DataTable();
            dr.Columns.Add("ID", typeof(int));
            dr.Columns.Add("Name",typeof(String));
            dr.Columns.Add("Status", typeof(string));
            dr.Columns.Add("CreateDate", typeof(DateTime));
            System.Data.DataRow drRow = dr.NewRow();
            drRow["ID"] = 1;
            drRow["Name"] = "test";
            drRow["Status"] = "open";
            drRow["CreateDate"] = DateTime.Now;
            dr.Rows.Add(drRow);

            System.Data.IDataReader idr = dr.CreateDataReader();

            using (mocks.Record())
            {
                dbprovider.Stub(x => x.ExecuteReader(null,null,null)).IgnoreArguments().Repeat.Any().Return(idr);
                Conn.Open();
                LastCall.Repeat.Any();
                Conn.Stub(x => x.InTrx).Repeat.Any().Return(false);
                Conn.Stub(x => x.dbProvider).Return((FFLib.Data.DBProviders.IDBProvider)dbprovider).Repeat.Any();
                Conn.Stub(x => x.State).Repeat.Any().Return(System.Data.ConnectionState.Closed);
            }

            using (mocks.Playback())
            {
                dtoTable = new FFLib.Data.DBTable<TestDTO>(Conn);
                TestDTO result = dtoTable.Load(1);
                Assert.AreEqual(expected.ID, result.ID);
            }

        }

        [Test]
        public void LoadWithFieldMap()
        {
            MockRepository mocks = new MockRepository();
            FFLib.Data.DBProviders.IDBProvider dbprovider = mocks.Stub<FFLib.Data.DBProviders.IDBProvider>();
            FFLib.Data.IDBConnection Conn = mocks.Stub<FFLib.Data.IDBConnection>();
            FFLib.Data.DBTable<TestDTO2> dtoTable;
            TestDTO2 expected = new TestDTO2();
            expected.XID = 1;


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

            using (mocks.Record())
            {
                dbprovider.Stub(x => x.ExecuteReader(null, null, null)).IgnoreArguments().Repeat.Any().Return(idr);
                Conn.Open();
                LastCall.Repeat.Any();
                Conn.Stub(x => x.InTrx).Repeat.Any().Return(false);
                Conn.Stub(x => x.dbProvider).Return((FFLib.Data.DBProviders.IDBProvider)dbprovider).Repeat.Any();
                Conn.Stub(x => x.State).Repeat.Any().Return(System.Data.ConnectionState.Closed);
            }

            using (mocks.Playback())
            {
                dtoTable = new FFLib.Data.DBTable<TestDTO2>(Conn);
                TestDTO2 result = dtoTable.Load(1);
                Assert.AreEqual(expected.XID, result.XID);
            }

        }

        [Test]
        public void Save()
        {
            MockRepository mocks = new MockRepository();
            FFLib.Data.DBProviders.IDBProvider dbprovider = mocks.DynamicMock<FFLib.Data.DBProviders.IDBProvider>();
            FFLib.Data.IDBConnection Conn = mocks.Stub<FFLib.Data.IDBConnection>();
            FFLib.Data.DBTable<TestDTO> dtoTable;
            TestDTO testData = new TestDTO();
            testData.ID = 1;
            testData.Name = "Test";
            testData.Status = "Open";
            testData.CreateDate = DateTime.Today;

            using (mocks.Record())
            {
                dbprovider.Expect(x => x.DBUpdate(Arg<FFLib.Data.IDBConnection>.Is.Anything
                    , Arg<string>.Is.Equal("UPDATE #__TableName SET \n[IsLocked] = @p1,[ID] = @p2,[Name] = @p3,[Status] = @p4,[CreateDate] = @p5\n WHERE [ID] = @pk")
                    , Arg<System.Data.SqlClient.SqlParameter[]>.Is.Anything));
                //dbprovider.Stub(x => x.ExecuteReader(null, null, null)).IgnoreArguments().Repeat.Any().Return(idr);
                Conn.Open();
                LastCall.Repeat.Any();
                Conn.Stub(x => x.InTrx).Repeat.Any().Return(false);
                Conn.Stub(x => x.dbProvider).Return((FFLib.Data.DBProviders.IDBProvider)dbprovider).Repeat.Any();
                Conn.Stub(x => x.State).Repeat.Any().Return(System.Data.ConnectionState.Closed);
            }

            using (mocks.Playback())
            {
                dtoTable = new FFLib.Data.DBTable<TestDTO>(Conn);
                dtoTable.Save(testData);
            }

        }

        [Test]
        public void SaveWithFieldMap()
        {
            MockRepository mocks = new MockRepository();
            FFLib.Data.DBProviders.IDBProvider dbprovider = mocks.DynamicMock<FFLib.Data.DBProviders.IDBProvider>();
            FFLib.Data.IDBConnection Conn = mocks.Stub<FFLib.Data.IDBConnection>();
            FFLib.Data.DBTable<TestDTO2> dtoTable;
            TestDTO2 testData = new TestDTO2();
            testData.XID = 1;
            testData.Name = "Test";
            testData.Status = "Open";
            testData.CreateDate = DateTime.Today;

            using (mocks.Record())
            {
                dbprovider.Expect(x => x.DBUpdate(Arg<FFLib.Data.IDBConnection>.Is.Anything
                    , Arg<string>.Is.Equal("UPDATE #__TableName SET \n[Locked] = @p1,[ID] = @p2,[Name] = @p3,[Status] = @p4,[CreateDate] = @p5\n WHERE [ID] = @pk")
                    , Arg<System.Data.SqlClient.SqlParameter[]>.Is.Anything));
                //dbprovider.Stub(x => x.ExecuteReader(null, null, null)).IgnoreArguments().Repeat.Any().Return(idr);
                Conn.Open();
                LastCall.Repeat.Any();
                Conn.Stub(x => x.InTrx).Repeat.Any().Return(false);
                Conn.Stub(x => x.dbProvider).Return((FFLib.Data.DBProviders.IDBProvider)dbprovider).Repeat.Any();
                Conn.Stub(x => x.State).Repeat.Any().Return(System.Data.ConnectionState.Closed);
            }

            using (mocks.Playback())
            {
                dtoTable = new FFLib.Data.DBTable<TestDTO2>(Conn);
                dtoTable.Save(testData);
            }

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