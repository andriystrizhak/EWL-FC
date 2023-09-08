using Eng_Flash_Cards_Learner;
using System.Data.SQLite;
using NUnit.Framework;

namespace EWL_Tests
{
    /// <summary>
    /// ���� �� ������ ����-�����
    /// </summary>
    public static class MyTestCases
    {
        /// <summary>
        /// ���� ����-����� � ����������
        /// </summary>
        /// <returns> IEnumerable �������� </returns>
        public static IEnumerable<TestCaseData> Categories_Cases()
        {
            yield return new TestCaseData("Category 1");
            yield return new TestCaseData("Category 2");
        }

        /// <summary>
        /// ���� ����-����� � �������
        /// </summary>
        /// <returns> IEnumerable ��� </returns>
        public static IEnumerable<TestCaseData> Words_Cases()
        {
            yield return new TestCaseData("a", "��");
            yield return new TestCaseData("b", "�");
            yield return new TestCaseData("c", "�");
        }
    }

    [TestFixture]
    public class DB_SQLite_Test
    {
        public DB_SQLite db { get; set; } = new DB_SQLite("Data Source=D:\\SELF-DEV\\HARD-SKILLS\\DEVELOPMENT\\PRACTICE\\MyProjects\\EWL FC\\EWL_Tests\\Test_DB.db;Version=3;");

        [SetUp]
        public void Setup()
            => TearDown();



        #region Words TESTS

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void NewWord_Adding_Test(string engW, string uaW)
        {
            Assert.IsTrue(db.TryAdd_Word_ToAllWords(engW, uaW),
                "����� �� ����� ������ ����� � AllWords");

            bool isAddedToAllWords = db.Get_DataReader($"SELECT * FROM AllWords " +
                $"WHERE EngWord = '{engW}' AND UaTranslation = '{uaW}'").HasRows;
            Assert.IsTrue(isAddedToAllWords,
                "������ ����� �� ���� ������ � ��");

            bool isWordAddedToWordCategories = db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = 3;").HasRows;
            Assert.IsTrue(isWordAddedToWordCategories,
                "������ ����� �� ���� ������ �� ������� '1' � WordCategories");
        }

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void TwoIdenticalWords_Adding_Test(string engW, string uaW)
        {
            Assert.IsTrue(db.TryAdd_Word_ToAllWords(engW, uaW),
                "����� �� ����� ������ ����� � AllWords");
            Assert.IsFalse(db.TryAdd_Word_ToAllWords(engW, uaW),
                "����� ����� ������ ����� � AllWords [� �� ������� ���]");
        }

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void LastWords_Removing_Test(string engW, string uaW)
        {
            db.TryAdd_Word_ToAllWords("dick", "����");       //��������� ������� �����
            db.TryAdd_Word_ToAllWords(engW, uaW);            //��������� ������� �����
            db.Remove_LastWords_Permanently(1);              //��������� ���������� �����

            //��������
            bool isLastAddedWordRemovedFromAllWords = !db.Get_DataReader($"SELECT * FROM AllWords WHERE EngWord = '{engW}'").HasRows;
            Assert.IsTrue(isLastAddedWordRemovedFromAllWords, "������ ����� �� ���� ��������");

            bool isAnotherWordNOTRemovedFromAllWords = db.Get_DataReader("SELECT * FROM AllWords WHERE EngWord = 'dick'").HasRows;
            Assert.IsTrue(isAnotherWordNOTRemovedFromAllWords, "���� ����� ���� �������� [� �� ������� ����]");

            bool isLastAddedWordRemovedFromWordCategories = !db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = 4;").HasRows;
            Assert.IsTrue(isLastAddedWordRemovedFromWordCategories, "������ ����� �� ���� �������� � WordCategories");

            bool isAnotherWordRemovedFromWordCategories = db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = 3;").HasRows;
            Assert.IsTrue(isAnotherWordRemovedFromWordCategories, "���� ����� ���� �������� � WordCategories [� �� ������� ����]");
        }

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void Word_Removing_Test(string engW, string uaW)
        {
            db.TryAdd_Word_ToAllWords("dick", "����");      //��������� ������� �����
            db.TryAdd_Word_ToAllWords(engW, uaW);           //��������� ������� �����
            db.Remove_Word_Permanently(4);                  //��������� ������� �����

            //��������
            bool isLastAddedWordRemovedFromAllWords = !db.Get_DataReader($"SELECT * FROM AllWords WHERE EngWord = '{engW}'").HasRows;
            Assert.IsTrue(isLastAddedWordRemovedFromAllWords, "����� ����� �� ���� ��������");

            bool isAnotherWordNOTRemovedFromAllWords = db.Get_DataReader("SELECT * FROM AllWords WHERE EngWord = 'dick'").HasRows;
            Assert.IsTrue(isAnotherWordNOTRemovedFromAllWords, "����� ����� ���� �������� [� �� ������� ����]");

            bool isLastAddedWordRemovedFromWordCategories = !db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = 4;").HasRows;
            Assert.IsTrue(isLastAddedWordRemovedFromWordCategories, "����� ����� �� ���� �������� � WordCategories");

            bool isAnotherWordRemovedFromWordCategories = db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = 3;").HasRows;
            Assert.IsTrue(isAnotherWordRemovedFromWordCategories, "����� ����� ���� �������� � WordCategories [� �� ������� ����]");
        }

        #endregion

        #region Categories [TESTS]

        [TestCase(2)]
        [TestCase(1)]
        public void CurrentCategory_SettingAndGetting_Test(int categoryID)
        {
            db.Set_CurrentCategory(categoryID);        //������������ �������� CurrentCategoryID � Settings

            //��������
            var reader = db.Get_DataReader("SELECT CurrentCategoryID FROM Settings");
            reader.Read();
            int actualCurrentCategoryID = reader.GetInt32(0);
            Assert.AreEqual(categoryID, actualCurrentCategoryID,
                "CurrentCategoryID �� ����������� �� ���������� ���������");

            int gettedCurrentCategoryID = db.Get_CurrentCategory();   //��������� �������� CurrentCategoryID � Settings
            int expectedCurrentCategoryID = actualCurrentCategoryID; //���� ����������� �����
            Assert.AreEqual(expectedCurrentCategoryID, gettedCurrentCategoryID,
                "�������� �������� CurrentCategoryID - �� ����");
        }

        [Test]
        public void AllCategories_Getting_Test()
        {
            var categories = db.Get_Categories();
            Assert.AreEqual("AllWords", categories[0].Name,
                "��������� ������ ���������� ��� ������� - �� �����");
        }

        #region Categories Adding [TESTS]

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void NewCategory_Adding_Test(string categoryName)
        {
            Assert.IsTrue(db.Add_NewCategory(categoryName),
                "��������� ���� ������� �� �����");
            bool isCategoryAdded = db.Get_DataReader($"SELECT * FROM Categories WHERE Name = '{categoryName}'").HasRows;
            Assert.IsTrue(isCategoryAdded,
                "���� �������� �� �'������� � Categories");
        }

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void TwoIdenticalCategories_Adding_Test(string categoryName)
        {
            Assert.IsTrue(db.Add_NewCategory(categoryName),
                "��������� ���� ������� �� �����");
            Assert.IsFalse(db.Add_NewCategory(categoryName),
                "��������� ���� ������� ����� [� �� ������� ����]");
        }

        #endregion

        #region Categories Marking as Removed [TESTS]

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void Category_MarkingAsRemoved_Test(string categoryName)
        {
            int categoryID = 3;
            db.Add_NewCategory(categoryName);          //��������� ���� ������� (3-��)
            db.Set_CurrentCategory(categoryID);        //������������ ������� ������� - '3'

            //��������
            Assert.IsTrue(db.TryMarkAsRemoved_Category(categoryID), 
                "������ �������� �� ���� (��������� ��) ��������");

            var reader = db.Get_DataReader($"SELECT Deleted, DeletedAt FROM Categories WHERE CategoryID = {categoryID}");
            reader.Read();
            bool categoryIsMarkedAsDeleted = reader.GetInt32(0) == 1;
            Assert.IsTrue(categoryIsMarkedAsDeleted, 
                "�������� ���� 'Deleted' ������ ������� �� �������� �� '1'");

            Assert.AreEqual(1, db.Get_CurrentCategory(), 
                "CurrentCategoryID � Settings �� �������� �� �������� '1'");

            string actualDateTime = reader.GetString(1);
            Assert.IsTrue(actualDateTime.Contains($"{DateTime.Now:yyyy-MM-dd HH:mm}"),
                "���� �� ����������� ��� ����������� ������");
        }

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void NonRemovableCategory_NOTMarkingAsRemoved_Test(string categoryName)
        {
            int categoryID = 3;
            db.Get_DataReader($"INSERT INTO Categories (Name, Deleted, CanBeDeleted) " +
                $"VALUES ('{categoryName}', 0, 0);");              //��������� �������, ��� �� ����� ��������

            //��������
            Assert.IsFalse(db.TryMarkAsRemoved_Category(categoryID), 
                "���� �������� ���� (��������� ��) �������� [� �� ������� ����]");

            var reader = db.Get_DataReader($"SELECT Deleted FROM Categories WHERE CategoryID = {categoryID}");
            reader.Read();
            bool categoryIsMarkedAsDeleted = reader.GetInt32(0) == 1;
            Assert.IsFalse(categoryIsMarkedAsDeleted,
                "�������� ���� 'Deleted' ������ ������� �������� �� '1' [� �� ������� ����]");
        }
        #endregion

        #region Categories Restoring from Deletion [TESTS]

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void Category_Restoring_FromDeletion_Test(string categoryName)
        {
            int categoryID = 3;
            db.Add_NewCategory(categoryName);           //��������� ���� ������� (3-��)
            db.Set_CurrentCategory(categoryID);         //������������ ������� ������� - '3'
            db.TryMarkAsRemoved_Category(categoryID);   //���������� ������� �� "��������"

            db.Restore_Category_FromDeletion(categoryID);   //³��������� ������� � "������"

            var reader = db.Get_DataReader($"SELECT Deleted FROM Categories WHERE CategoryID = {categoryID}");
            reader.Read();
            bool categoryIsMarkedAsDeleted = reader.GetInt32(0) == 1;
            Assert.IsFalse(categoryIsMarkedAsDeleted,
                "�������� ���� 'Deleted' ������ ������� �� �������� �� '0'");

            Assert.AreEqual(1, db.Get_CurrentCategory(),
                "CurrentCategoryID � Settings �������� � ���������� '1' �� ���� [� �� ������� ����]");
        }
        #endregion

        #region Categories Removing [TESTS]

        public (string query1, string query2) MarkedCategories_Removing_Test(string categoryName)
        {
            int categoryID = 3;
            db.Add_NewCategory(categoryName);           //��������� ���� ������� (3-��)
            db.TryAdd_Word_ToCategory(1, categoryID);   //��������� ������� ����� �� ���� �������
            db.TryAdd_Word_ToCategory(2, categoryID);   //��������� ������� ����� �� ���� �������
            db.TryMarkAsRemoved_Category(categoryID);   //���������� ������� �� "��������"

            return ($"SELECT * FROM WordCategories WHERE CategoryID = '{categoryID}'",
                $"SELECT * FROM Categories WHERE CategoryID = '{categoryID}'");
        }

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void RecentlyMarkedCategories_Removing_Test(string categoryName)
        {
            var queries = MarkedCategories_Removing_Test(categoryName);

            //��������
            db.FindAndRemove_LongMarkedCategories(3);

            bool isRecentlyMarkedCategoryWordsDeleted = !db.Get_DataReader(queries.query1).HasRows;
            Assert.IsFalse(isRecentlyMarkedCategoryWordsDeleted, 
                "����� ���� ������� ������� � WordCategory [� �� ������ ����]");

            bool isRecentlyMarkedCategoryDeleted = !db.Get_DataReader(queries.query2).HasRows;
            Assert.IsFalse(isRecentlyMarkedCategoryDeleted, 
                "���� �������� �������� � Categories [� �� ������� ����]");
        }

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void LongMarkedCategories_Removing_Test(string categoryName)
        {
            int categoryID = 3;
            var queries = MarkedCategories_Removing_Test(categoryName);

            //���� ���� (���������� ��) ��������� ���� �������
            db.Get_DataReader($"UPDATE Categories " +
                $"SET DeletedAt = '{new DateTime(2023, 01, 01):yyyy-MM-dd HH:mm:ss}' WHERE CategoryID = {categoryID}");
            db.FindAndRemove_LongMarkedCategories(3);

            //��������
            db.FindAndRemove_LongMarkedCategories(1);
            bool isLongMarkedCategoryWordsDeleted = !db.Get_DataReader(queries.query1).HasRows;
            Assert.IsTrue(isLongMarkedCategoryWordsDeleted,
                "����� ���� ������� �� ������� � WordCategory");

            bool isLongMarkedCategoryDeleted = !db.Get_DataReader(queries.query2).HasRows;
            Assert.IsTrue(isLongMarkedCategoryDeleted,
                "���� �������� �� �������� � Categories");
        }
        #endregion

        #endregion

        #region WordCategories [TESTS]

        [TestCase(1)]
        [TestCase(2)]
        public void Word_Adding_ToCategory_Test(int wordID)
        {
            int anotherCategoryID = 2;

            //��������
            Assert.IsTrue(db.TryAdd_Word_ToCategory(wordID, anotherCategoryID), 
                "����� �� ����� ����� �� ������� '2'");

            bool isAddedToCategory2 = db.Get_DataReader($"SELECT * FROM WordCategories " +
                $"WHERE WordID = {wordID} AND CategoryID = {anotherCategoryID};").HasRows;
            Assert.IsTrue(isAddedToCategory2, "����� �� �'������� � ������� '2'");
        }

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void TwoIdenticalWords_Adding_ToCategory_Test(string engW, string uaW)
        {
            int anotherWordID = 2;
            int anotherCategoryID = 2;

            Assert.IsTrue(db.TryAdd_Word_ToCategory(anotherWordID, anotherCategoryID), 
                "����� �� ����� ����� �� ������� '2'");
            Assert.IsFalse(db.TryAdd_Word_ToCategory(anotherWordID, anotherCategoryID), 
                "����� ����� ��������� ����� �� ������� '2' [� �� �������]");
        }

        /// <summary>
        /// �������� �� ������� ���������� ���� ������ �� ������� ����� � ������������ ������� wordID �� categoryID
        /// </summary>
        /// <param name="invalidNumber">������������ ����� ��� wordID �� categoryID</param>
        [TestCase(0)]
        [TestCase(-2)]
        public void InvalidNumber_Word_Adding_ToCategory_Test(int invalidNumber)
        {
            int validWordID = 2;
            int validCategoryID = 2;

            TestDelegate AddValidWordIDToInvalidCategoryID = () => db.TryAdd_Word_ToCategory(validWordID, invalidNumber);
            Assert.Catch(typeof(ArgumentException), AddValidWordIDToInvalidCategoryID,
                "��� ������� �������� ����������, � �� ������");

            TestDelegate AddInvalidWordIDToValidCategoryID = () => db.TryAdd_Word_ToCategory(invalidNumber, validCategoryID);
            Assert.Catch(typeof(ArgumentException), AddInvalidWordIDToValidCategoryID,
                "��� ������� �������� ����������, � �� ������");

            TestDelegate AddInvalidWordIDToInvalidCategoryID = () => db.TryAdd_Word_ToCategory(invalidNumber, invalidNumber);
            Assert.Catch(typeof(ArgumentException), AddInvalidWordIDToInvalidCategoryID,
                "��� ������� �������� ����������, � �� ������");

        }

        [TestCase(1)]
        [TestCase(2)]
        public void LastAddedWord_Removing_FromWordCategories_Test(int count)
        {
            int categoryID = 2;
            for (int i = 1; i <= count; i++)
                db.TryAdd_Word_ToCategory(i, categoryID);        //��������� �����(-��) �� ���� �������
            db.Remove_LastWord_FromCategory(count);     //��������� ����������(-��) �������� �� ���� ������� ����� � WordCategories

            int allWordsCount = 2;
            for (int i = 1; i <= allWordsCount; i++)
            {
                if (i <= count)
                {
                    bool isLastAddedWordRemovedFromAllWords = !db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = {i} AND CategoryID = 2").HasRows;
                    Assert.IsTrue(isLastAddedWordRemovedFromAllWords, "������ ����� ����� �� ���� �������");
                }
                else
                {
                    bool isAnotherWordNOTRemovedFromAllWords = db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = {i} AND CategoryID = 1").HasRows;
                    Assert.IsTrue(isAnotherWordNOTRemovedFromAllWords, "���� ����� ���� ������� [� �� ������ ����]");
                }
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        public void Word_Removing_FromWordCategories_Test(int wordID)
        {
            //TODO - ���������� ���� "��������� ����� � �������"
            int categoryID = 2;                                  //����� ��������� �������
            db.TryAdd_Word_ToCategory(wordID, categoryID);       //��������� �����(-��) �� ���� �������
            db.Remove_Word_FromCategory(wordID, 1);              //������ ��������� ����� � ������� �������

            //��������
            bool isWordNOTRemovedFromAllWordsCategory = db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = {wordID} AND CategoryID = 1").HasRows;
            Assert.IsTrue(isWordNOTRemovedFromAllWordsCategory, "C���� ���� �������� � ������� ������� [� �� ������� ����]");

            db.Remove_Word_FromCategory(wordID, categoryID);     //��������� ����� � ��������� �������

            //��������
            bool isWordRemovedFromAnotherCategory = !db.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = {wordID} AND CategoryID = 2").HasRows;
            Assert.IsTrue(isWordRemovedFromAnotherCategory, "����� �� ���� �������� � ��������� �������");
        }


        #region WordCategories - Get Words from Category [TESTS]

        /// <summary>
        /// ��������� ��� �� �� (� �������� � ��� �� ������ ������� '2')
        /// </summary>
        /// <param name="wordCount">ʳ������ ���</param>
        void Words_Adding_ToDB(int wordCount)
        {
            int anotherCategoryID = 2;
            int alreadyAddedWordsCount = 2;

            for (int i = 0; i < wordCount; i++)
                db.TryAdd_Word_ToAllWords($"{i + 1}", "i + 1");
            for (int i = 0; i < wordCount + alreadyAddedWordsCount; i++)
                if (i % 2 == 0)
                    db.TryAdd_Word_ToCategory(i + 1, anotherCategoryID);
        }

        [TestCase(4)]
        [TestCase(6)]
        public void Words_Getting_FromMainCategory_Test(int addedWordCount)
        {
            int getWordsCount = 4;
            int categoryID = 1;
            Words_Adding_ToDB(addedWordCount);

            var allWFromCategory = db.Get_Words_FromCategory(categoryID);
            Assert.AreEqual(addedWordCount + 2, allWFromCategory.Count, 
                $"��������� ��� �� ���� {addedWordCount + 2} � �� {allWFromCategory.Count}");

            var countWFromCategory = db.Get_Words_FromCategory(categoryID, getWordsCount);
            Assert.AreEqual(getWordsCount, countWFromCategory.Count, 
                $"��������� ��� �� ���� {getWordsCount} � �� {allWFromCategory.Count}");
        }

        [TestCase(4)]
        [TestCase(6)]
        public void Words_Getting_FromAnotherCategory_Test(int addedWordCount)
        {
            int getWordsCount = 2;
            int categoryID = 2;
            Words_Adding_ToDB(addedWordCount);

            var allWFromCategory = db.Get_Words_FromCategory(categoryID);
            //int addedWordCount
            Assert.AreEqual((int)Math.Round(((double)addedWordCount + 2)/2, MidpointRounding.AwayFromZero), allWFromCategory.Count,
                $"��������� ��� �� ���� {addedWordCount + 2} � �� {allWFromCategory.Count}");

            var countWFromCategory = db.Get_Words_FromCategory(categoryID, getWordsCount);
            Assert.AreEqual(getWordsCount, countWFromCategory.Count,
                $"��������� ��� �� ���� {addedWordCount + 2} � �� {allWFromCategory.Count}");   
        }

        [TestCase(4)]
        [TestCase(6)]
        public void Words_NOTGetting_FromAnotherCategory_Test(int addedWordCount)
        {
            Words_Adding_ToDB(addedWordCount);
            int nonexistentWordCount = -2;
            TestDelegate GetWordsFromNonexistentCategory = () => db.Get_Words_FromCategory(2, nonexistentWordCount);

            Assert.Catch(typeof(ArgumentException), GetWordsFromNonexistentCategory, 
                "��� ������� �������� ����������, � �� ������");
        }

        #endregion

        #endregion

        //TODO - �������� ����� ...






        [TearDown]
        public void TearDown()
        {
            db.Get_DataReader("DELETE FROM WordCategories WHERE AddedAt NOT IN ( SELECT AddedAt FROM WordCategories ORDER BY AddedAt LIMIT 2);");
            db.Get_DataReader("DELETE FROM AllWords WHERE WordID NOT IN ( SELECT WordID FROM AllWords ORDER BY WordID LIMIT 2);");
            db.Get_DataReader("DELETE FROM Categories WHERE CategoryID NOT IN ( SELECT CategoryID FROM Categories ORDER BY CategoryID LIMIT 2);");
        }

        /*
        [TestCase("Programming Table", "ProgrammingTable")]
        [TestCase("programming table", "ProgrammingTable")]
        [TestCase("wild Animals", "WildAnimals")]
        [TestCase("daily vocab", "DailyVocab")]
        public void CategoryTableNameCreation_Test(string categoryName, string categoryTableName)
        {
            string createdCategoryTableName = string.Concat(categoryName.Split().Select(c => char.ToUpper(c[0]) + c.Substring(1)));
            Assert.AreEqual(categoryTableName, createdCategoryTableName);
        }
        */
    }
}