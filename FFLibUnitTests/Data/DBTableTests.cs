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
            //System.Data.IDataReader dr = new Mocks.IDataReader();
            FFLib.Data.DBTable<TestDTO> dtoTable;
            TestDTO expected = new TestDTO();
            expected.ID = 1;
            //Mocks.IDataReader.ResultSet dataset = new Mocks.IDataReader.ResultSet(new string[] { "ID", "Name", "Status", "CreateDate" }, new object[][] { new object[] { 1, "test", "open", DateTime.Now } });
            //((Mocks.IDataReader)dr).ResultSets.Add(dataset);
            //System.Data.IDataReader dr = MockRepository.GenerateStub<System.Data.IDataReader>();
            //dr.Stub(x => x.Read()).Repeat.Once().Return(true);
            //dr.Stub(x => x.Read()).Return(false);
            //dr.Stub(x => x.IsClosed).Return(true).Repeat.Any();
            //dr.Stub(x => x["ID"]).Return(1);
            //dr.Stub(x => x["Status"]).Return("open");
            //dr.Stub(x => x["Name"]).Return("Test Name");
            //dr.Stub(x => x["CreateDate"]).Return(DateTime.Now );

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
            bool st = idr.IsClosed;

            using (mocks.Record())
            {
                dbprovider.Stub(x => x.ExecuteReader(null,null,null)).IgnoreArguments().Repeat.Any().Return(idr);
                Conn.Open();
                LastCall.Repeat.Any();
                Conn.Stub(x => x.InTrx).Repeat.Any().Return(false);
                Conn.Stub(x => x.dbProvider).Return((FFLib.Data.DBProviders.IDBProvider)dbprovider).Repeat.Any();
                Conn.Stub(x => x.State).Repeat.Any().Return(System.Data.ConnectionState.Closed);
                //mocks.Replay(Conn);
                

            }

            using (mocks.Playback())
            {
                dtoTable = new FFLib.Data.DBTable<TestDTO>(Conn);
                TestDTO result = dtoTable.Load(1);
                Assert.AreEqual(expected.ID, result.ID);
            }

        }
    }

    class TestDTO
    {
        public int ID;
        public string Name;
        public string Status;
        public DateTime CreateDate;
    }
}