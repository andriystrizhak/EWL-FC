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


        [TestCase("mummy", "������")]
        [TestCase("daddy", "������")]
        [TestCase("granny", "������")]
        public void NewWordAdding_Test(string engW, string uaW)
        {
            //��������� �����
            if (!db.TryAddWord_ToAllWords(engW, uaW)) Assert.Fail();

            //�������� �� ����� ��� � ���
            var reader = db.GetDataReader("SELECT * FROM AllWords");
            reader.Read();
            Assert.AreEqual(reader.GetString(1), engW);
            Assert.AreEqual(reader.GetString(2), uaW);

            //�������� �� �� �� ���� ������ ���� ����� �� ������� �������
            var isWordAddedToWordCategories = db.GetDataReader($"SELECT * FROM WordCategories WHERE WordID = '{reader.GetInt32(0)}';").HasRows;
            Assert.IsTrue(isWordAddedToWordCategories);
        }

        [TestCase("mummy", "������")]
        [TestCase("daddy", "������")]
        [TestCase("granny", "������")]
        public void LastWordsRemoving_Test(string engW, string uaW)
        {
            //��������� ���� ���
            db.TryAddWord_ToAllWords("dick", "����");
            //Thread.Sleep(1000);
            db.TryAddWord_ToAllWords(engW, uaW);
            //��������� ���������� �����
            db.RemoveLastWord_Completely(1);

            //�������� �� ��: �� ���� ������ ������ ����� ��������
            bool isLastAddedWordRemovedFromAllWords = !db.GetDataReader($"SELECT * FROM AllWords WHERE EngWord = '{engW}'").HasRows;
            Assert.IsTrue(isLastAddedWordRemovedFromAllWords);
            //�������� �� ��: �� �� ���� ���� ����� ��������
            bool isAnotherWordNOTRemovedFromAllWords = db.GetDataReader("SELECT * FROM AllWords WHERE EngWord = 'dick'").HasRows;
            Assert.IsTrue(isAnotherWordNOTRemovedFromAllWords);
            //�������� �� ��: �� ���� ������ ������ ����� �������� � ��������
            bool isLastAddedWordRemovedFromWordCategories = !db.GetDataReader($"SELECT * FROM WordCategories WHERE WordID = 2;").HasRows;
            Assert.IsTrue(isLastAddedWordRemovedFromWordCategories);
            //�������� �� ��: �� ���� ���� ����� �������� � ��������
            bool isAnotherWordRemovedFromWordCategories = db.GetDataReader($"SELECT * FROM WordCategories WHERE WordID = 1;").HasRows;
            Assert.IsTrue(isAnotherWordRemovedFromWordCategories);
        }





        [TearDown]
        public void TearDown()
        {
            db.GetDataReader($"DELETE FROM WordCategories");
            db.GetDataReader($"DELETE FROM AllWords");
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