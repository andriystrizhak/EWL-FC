using Eng_Flash_Cards_Learner;

namespace EWL_Tests
{
    [TestFixture]
    public class DB_SQLite_Test
    {
        public DB_SQLite db { get; set; } = new DB_SQLite("Data Source=D:\\SELF-DEV\\HARD-SKILLS\\DEVELOPMENT\\PRACTICE\\MyProjects\\EWL FC\\EWL_Tests\\Test_DB.db;Version=3;");

        [SetUp]
        public void Setup()
        {
            TearDown();
        }


        #region Words TESTS

        [TestCase("mummy", "������")]
        [TestCase("daddy", "������")]
        [TestCase("granny", "������")]
        public void NewWord_Adding_Test(string engW, string uaW)
        {
            //�������� �� ����� ��������� �����
            Assert.IsTrue(db.TryAdd_Word_ToAllWords(engW, uaW));

            //�������� �� ����� ��� � ���
            var reader = db.Get_DataReader($"SELECT * FROM AllWords WHERE EngWord = '{engW}'");
            reader.Read();
            Assert.AreEqual(engW, reader.GetString(1));
            Assert.AreEqual(uaW, reader.GetString(2));

            //�������� �� �� �� ���� ������ ���� ����� �� ������� �������
            bool isWordAddedToWordCategories = db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = '{reader.GetInt32(0)}';").HasRows;
            Assert.IsTrue(isWordAddedToWordCategories);
        }

        [TestCase("mummy", "������")]
        [TestCase("daddy", "������")]
        [TestCase("granny", "������")]
        public void TwoIdenticalWords_Adding_Test(string engW, string uaW)
        {
            //�������� �� ����� ��������� �����
            Assert.IsTrue(db.TryAdd_Word_ToAllWords(engW, uaW));
            //�������� �� ������� �������� ��������� �����
            Assert.IsFalse(db.TryAdd_Word_ToAllWords(engW, uaW));
        }

        [TestCase("mummy", "������")]
        [TestCase("daddy", "������")]
        [TestCase("granny", "������")]
        public void LastWords_Removing_Test(string engW, string uaW)
        {
            //��������� ���� ���
            db.TryAdd_Word_ToAllWords("dick", "����");
            //Thread.Sleep(1000);
            db.TryAdd_Word_ToAllWords(engW, uaW);
            //��������� ���������� �����
            db.Remove_LastWords_FromAllWords(1);

            //�������� �� ��: �� ���� ������ ������ ����� ��������
            bool isLastAddedWordRemovedFromAllWords = !db.Get_DataReader($"SELECT * FROM AllWords WHERE EngWord = '{engW}'").HasRows;
            Assert.IsTrue(isLastAddedWordRemovedFromAllWords);
            //�������� �� ��: �� �� ���� ���� ����� ��������
            bool isAnotherWordNOTRemovedFromAllWords = db.Get_DataReader("SELECT * FROM AllWords WHERE EngWord = 'dick'").HasRows;
            Assert.IsTrue(isAnotherWordNOTRemovedFromAllWords);
            //�������� �� ��: �� ���� ������ ������ ����� �������� � ��������
            bool isLastAddedWordRemovedFromWordCategories = !db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = 4;").HasRows;
            Assert.IsTrue(isLastAddedWordRemovedFromWordCategories);
            //�������� �� ��: �� ���� ���� ����� �������� � ��������
            bool isAnotherWordRemovedFromWordCategories = db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = 3;").HasRows;
            Assert.IsTrue(isAnotherWordRemovedFromWordCategories);
        }
        #endregion

        #region Categories TESTS

        [TestCase(2)]
        [TestCase(1)]
        public void CurrentCategory_SettingAndGetting_Test(int categoryID)
        {
            //������������ �������� CurrentCategoryID � Settings
            db.Set_CurrentCategory(categoryID);
            //�������� �� ���� CurrentCategoryID � Settings
            var reader = db.Get_DataReader("SELECT CurrentCategoryID FROM Settings");
            reader.Read();
            int actualCurrentCategoryID = reader.GetInt32(0);
            Assert.AreEqual(categoryID, actualCurrentCategoryID);

            //��������� �������� CurrentCategoryID � Settings
            int gettedCurrentCategoryID = db.Get_CurrentCategory();
            //�������� �� �� �� �������� CurrentCategoryID ��������� ����
            int expectedCurrentCategoryID = actualCurrentCategoryID; //���� ����������� �����
            Assert.AreEqual(expectedCurrentCategoryID, gettedCurrentCategoryID);
        }

        [Test]
        public void AllCategories_Getting_Test()
        {
            var categories = db.Get_Categories();
            Assert.AreEqual("AllWords", categories[0].Name);
        }

        [TestCase("Category 1")]
        [TestCase("Category 2")]
        public void NewCategory_Adding_Test(string categoryName)
        {
            //�������� �� ����� ��������� �������
            Assert.IsTrue(db.Add_NewCategory(categoryName));
            //�������� �� ����� ������� � Categories
            bool isCategoryAdded = db.Get_DataReader($"SELECT * FROM Categories WHERE Name = '{categoryName}'").HasRows;
            Assert.IsTrue(isCategoryAdded);
        }

        [TestCase("Category 1")]
        [TestCase("Category 2")]
        public void TwoIdenticalCategories_Adding_Test(string categoryName)
        {
            //�������� �� ����� ��������� �������
            Assert.IsTrue(db.Add_NewCategory(categoryName));
            //�������� �� ������� �������� ��������� �������
            Assert.IsFalse(db.Add_NewCategory(categoryName));
        }
        #endregion

        #region WordCategories TESTS

        [TestCase("a", "��")]
        [TestCase("b", "�")]
        [TestCase("c", "�")]
        public void Word_Adding_ToCategory_Test(string engW, string uaW)
        {
            //�������� �� ����� ��������� ����� �� ���� �������
            Assert.IsTrue(db.TryAdd_Word_ToCategory(2, 2));

            //�������� �� ����� ����� � ����� ������� � WordCategories
            bool isAddedToCategory2 = db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = 2 AND CategoryID = 2;").HasRows;
            Assert.IsTrue(isAddedToCategory2);
        }

        [TestCase("mummy", "������")]
        [TestCase("daddy", "������")]
        [TestCase("granny", "������")]
        public void TwoIdenticalWords_Adding_ToCategory_Test(string engW, string uaW)
        {
            //�������� �� ����� ��������� ����� �� ���� �������
            Assert.IsTrue(db.TryAdd_Word_ToCategory(2, 2));
            //�������� �� ������� ��������� ����� �� ���� �������
            Assert.IsFalse(db.TryAdd_Word_ToCategory(2, 2));
        }

        [TestCase(1)]
        [TestCase(2)]
        public void Word_Removing_FromWordCategories_Test(int count)
        {
            //��������� �����(-��) �� ���� �������
            for (int i = 1; i <= count; i++)
                db.TryAdd_Word_ToCategory(i, 2);
            //��������� ����������(-��) �������� �� ���� ������� ����� � WordCategories
            db.Remove_LastWord_FromCategory(count);

            int allWordsCount = 2;
            for (int i = 1; i <= allWordsCount; i++)
            {
                if (i <= count)
                {
                    //�������� �� ��: �� ���� ������ ����� ����� �������i
                    bool isLastAddedWordRemovedFromAllWords = !db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = {i} AND CategoryID = 2").HasRows;
                    Assert.IsTrue(isLastAddedWordRemovedFromAllWords);
                }
                else
                {
                    //�������� �� ��: �� �� ���� ���� ����� �������
                    bool isAnotherWordNOTRemovedFromAllWords = db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = {i} AND CategoryID = 1").HasRows;
                    Assert.IsTrue(isAnotherWordNOTRemovedFromAllWords);
                }
            }
        }

        #endregion







        [TearDown]
        public void TearDown()
        {
            db.Get_DataReader("DELETE FROM WordCategories WHERE AddedAt NOT IN ( SELECT AddedAt FROM WordCategories ORDER BY AddedAt LIMIT 2);");
            db.Get_DataReader("DELETE FROM AllWords WHERE WordID NOT IN ( SELECT WordID FROM AllWords ORDER BY WordID LIMIT 2);");
            db.Get_DataReader("DELETE FROM Categories WHERE CategoryID NOT IN ( SELECT CategoryID FROM Categories ORDER BY CategoryID LIMIT 2);");
        }







        [TestCase("Programming Table", "ProgrammingTable")]
        [TestCase("programming table", "ProgrammingTable")]
        [TestCase("wild Animals", "WildAnimals")]
        [TestCase("daily vocab", "DailyVocab")]
        public void CategoryTableNameCreation_Test(string categoryName, string categoryTableName)
        {
            string createdCategoryTableName = string.Concat(categoryName.Split().Select(c => char.ToUpper(c[0]) + c.Substring(1)));
            Assert.AreEqual(categoryTableName, createdCategoryTableName);
        }
    }
}