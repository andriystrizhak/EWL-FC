using Eng_Flash_Cards_Learner.EF_SQLite;
using System.Data.SQLite;
using NUnit.Framework;
using Eng_Flash_Cards_Learner;
using SQLitePCL;

namespace EWL_Tests
{
    [TestFixture]
    public class SQLs_Test
    {
        //�� ������� ��������� ����� � ��ò������� � �������Ҳ ����˲

        /// <summary>
        /// Connection String (contains path to .db file)
        /// </summary>
        string conStr = "Data Source=D:\\SELF-DEV\\HARD-SKILLS\\DEVELOPMENT\\PRACTICE\\MyProjects\\EWL FC\\EWL_Tests\\Test_SQLs.db;";

        [OneTimeSetUp]
        public void Setup()
        {
            SQLs.ConStr = conStr;
            //VocabularyContext.DoLogActions = false;
            TearDown();
        }


        #region [ Words ]

        #region Words Adding

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void NewWord_Adding_Test(string engW, string uaW)
        {
            Assert.IsTrue(SQLs.TryAdd_Word_ToAllWords(engW, uaW),
                "����� �� ����� ������ ����� � Words");

            using VocabularyContext db = new(conStr);

            bool isAddedToWords = db.AllWords.Any(c => c.EngWord == engW && c.UaTranslation == uaW);
            Assert.IsTrue(isAddedToWords,
                "������ ����� �� ���� ������ � ��");

            bool isWordAddedToWordCategories = db.WordCategories.Any(c => c.WordId == 3);
            Assert.IsTrue(isWordAddedToWordCategories,
                "������ ����� �� ���� ������ �� ������� '1' � WordCategories");
        }

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void TwoIdenticalWords_Adding_Test(string engW, string uaW)
        {
            Assert.IsTrue(SQLs.TryAdd_Word_ToAllWords(engW, uaW),
                "����� �� ����� ������ ����� � Words");
            Assert.IsFalse(SQLs.TryAdd_Word_ToAllWords(engW, uaW),
                "����� ����� ������ ����� � Words [� �� ������� ���]");
        }

        #endregion

        #region Words Removing

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void LastWords_Removing_Test(string engW, string uaW)
        {
            var (anotherWId, anotherEngW, AnotherUaW) = (3, "candle", "�����");
            SQLs.TryAdd_Word_ToAllWords(anotherEngW, AnotherUaW);   //��������� ������� �����
            SQLs.TryAdd_Word_ToAllWords(engW, uaW);                 //��������� ������� �����
            SQLs.Remove_LastWords_Permanently(1);                   //��������� ���������� �����

            using VocabularyContext db = new(conStr);

            //��������
            bool isLastAddedWordRemovedFromWords = !db.AllWords.Any(c => c.EngWord == engW);
            Assert.IsTrue(isLastAddedWordRemovedFromWords, "������ ����� �� ���� ��������");

            bool isAnotherWordNOTRemovedFromWords = db.AllWords.Any(c => c.EngWord == anotherEngW);
            Assert.IsTrue(isAnotherWordNOTRemovedFromWords, "���� ����� ���� �������� [� �� ������� ����]");

            bool isLastAddedWordRemovedFromWordCategories = !db.WordCategories.Any(c => c.WordId == 4);
            Assert.IsTrue(isLastAddedWordRemovedFromWordCategories, "������ ����� �� ���� �������� � WordCategories");

            bool isAnotherWordRemovedFromWordCategories = db.WordCategories.Any(c => c.WordId == anotherWId);
            Assert.IsTrue(isAnotherWordRemovedFromWordCategories, "���� ����� ���� �������� � WordCategories [� �� ������� ����]");
        }

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void Word_Removing_Test(string engW, string uaW)
        {
            var (anotherWId, anotherEngW, AnotherUaW) = (3, "candle", "�����");
            SQLs.TryAdd_Word_ToAllWords(anotherEngW, AnotherUaW);   //��������� ������� �����
            SQLs.TryAdd_Word_ToAllWords(engW, uaW);                 //��������� ������� �����
            SQLs.Remove_Word_Permanently(4);                        //��������� ������� �����

            using VocabularyContext db = new(conStr);

            //��������
            bool isLastAddedWordRemovedFromWords = !db.AllWords.Any(c => c.EngWord == engW);
            Assert.IsTrue(isLastAddedWordRemovedFromWords, "����� ����� �� ���� ��������");

            bool isAnotherWordNOTRemovedFromWords = db.AllWords.Any(c => c.EngWord == anotherEngW);
            Assert.IsTrue(isAnotherWordNOTRemovedFromWords, "����� ����� ���� �������� [� �� ������� ����]");

            bool isLastAddedWordRemovedFromWordCategories = !db.WordCategories.Any(c => c.WordId == 4);
            Assert.IsTrue(isLastAddedWordRemovedFromWordCategories, "����� ����� �� ���� �������� � WordCategories");

            bool isAnotherWordRemovedFromWordCategories = db.WordCategories.Any(c => c.WordId == anotherWId);
            Assert.IsTrue(isAnotherWordRemovedFromWordCategories, "����� ����� ���� �������� � WordCategories [� �� ������� ����]");
        }

        #endregion

        #region Changing word parametrs

        [TestCase(1, 4)]
        [TestCase(1, 0)]
        public void WordRating_Setting_Test(int wordId, int rating)
        {
            SQLs.RateWord(wordId, rating);        //������������ �������� Rating � Word

            using VocabularyContext db = new(conStr);

            var actualRating = db.AllWords.First(w => w.WordId == wordId).Rating;
            Assert.AreEqual(rating, actualRating,
                "WordAddingMode �� ����������� �� ���������� ���������");
        }

        [TestCase(0, 4)]
        [TestCase(1, -1)]
        public void WordAddingMode_NOTSettingBelowOne_Test(int wordId, int rating)
        {
            //������������ �������� Rating � Word
            Assert.Catch(typeof(ArgumentException), () => SQLs.RateWord(wordId, rating),
                "��� ������� �������� ����������, � �� ������");
        }


        [TestCase(1)]
        [TestCase(2)]
        public void WordRepetition_Incrementation_Test(int wordId)
        {
            int repetition;
            using (VocabularyContext db = new(conStr))
            {
                repetition = db.AllWords.First(w => w.WordId == wordId).Repetition;
                SQLs.IncrementWordRepetition(wordId);        //��������� �������� Repetition � Word
            }
            using (VocabularyContext db = new(conStr))
            {
                var actualRepetition = db.AllWords.First(w => w.WordId == wordId).Repetition;
                Assert.AreEqual(repetition + 1, actualRepetition,
                    "Repetition �� ���������� �� ���������� ���������");

                db.AllWords.First(w => w.WordId == wordId).Repetition = 0;
                db.SaveChanges();
            }
        }

        #endregion

        #endregion

        #region [ Categories ]

        #region Categories Adding

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void NewCategory_Adding_Test(string categoryName)
        {
            Assert.IsTrue(SQLs.Add_NewCategory(categoryName),
                "��������� ���� ������� �� �����");

            using VocabularyContext db = new(conStr);

            bool isCategoryAdded = db.Categories.Any(c => c.Name == categoryName);
            Assert.IsTrue(isCategoryAdded,
                "���� �������� �� �'������� � Categories");
        }

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void TwoIdenticalCategories_Adding_Test(string categoryName)
        {
            Assert.IsTrue(SQLs.Add_NewCategory(categoryName),
                "��������� ���� ������� �� �����");
            Assert.IsFalse(SQLs.Add_NewCategory(categoryName),
                "��������� ���� ������� ����� [� �� ������� ����]");
        }

        #endregion

        #region Categories Marking as Removed

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void Category_MarkingAsRemoved_Test(string categoryName)
        {
            int categoryID = 3;
            SQLs.Add_NewCategory(categoryName);          //��������� ���� ������� (3-��)
            SQLs.Set_CurrentCategory(categoryID);        //������������ ������� ������� - '3'

            using VocabularyContext db = new(conStr);

            //��������
            Assert.IsTrue(SQLs.TryMarkAsRemoved_Category(categoryID),
            "������ �������� �� ���� (��������� ��) ��������");

            var category = db.Categories.First(c => c.CategoryId == categoryID);
            bool categoryIsMarkedAsDeleted = category.Deleted;
            Assert.IsTrue(categoryIsMarkedAsDeleted,
                "�������� ���� 'Deleted' ������ ������� �� �������� �� '1'");

            Assert.AreEqual(1, db.Settings.First().CurrentCategoryId,
                "CurrentCategoryID � Settings �� �������� �� �������� '1'");

            DateTime actualDateTime = category.DeletedAt;
            Assert.IsTrue(($"{actualDateTime:yyyy-MM-dd HH:mm}" == $"{DateTime.Now:yyyy-MM-dd HH:mm}"),
                "���� �� ����������� ��� ����������� ������");
        }

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void NonRemovableCategory_NOTMarkingAsRemoved_Test(string categoryName)
        {
            using VocabularyContext db = new(conStr);

            int categoryID = 3;
            //��������� �������, ��� �� ����� ��������
            db.Categories.Add(new Category { Name = categoryName, CanBeDeleted = false });
            db.SaveChanges();

            //��������
            Assert.IsFalse(SQLs.TryMarkAsRemoved_Category(categoryID),
                "���� �������� ���� (��������� ��) �������� [� �� ������� ����]");

            bool categoryIsMarkedAsDeleted = db.Categories.First(c => c.CategoryId == categoryID).Deleted;
            Assert.IsFalse(categoryIsMarkedAsDeleted,
                "�������� ���� 'Deleted' ������ ������� �������� �� '1' [� �� ������� ����]");
        }
        #endregion

        #region Categories Restoring from Deletion

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void Category_Restoring_FromDeletion_Test(string categoryName)
        {
            int categoryID = 3;
            SQLs.Add_NewCategory(categoryName);           //��������� ���� ������� (3-��)
            SQLs.Set_CurrentCategory(categoryID);         //������������ ������� ������� - '3'
            SQLs.TryMarkAsRemoved_Category(categoryID);   //���������� ������� �� "��������"

            SQLs.Restore_Category_FromDeletion(categoryID);   //³��������� ������� � "������"

            using VocabularyContext db = new(conStr);

            //var reader = DB.Get_DataReader($"SELECT Deleted FROM Categories WHERE CategoryID = {categoryID}");
            //reader.Read();
            bool categoryIsMarkedAsDeleted = db.Categories.First(c => c.CategoryId == categoryID).Deleted; //reader.GetInt32(0) == 1;
            Assert.IsFalse(categoryIsMarkedAsDeleted,
                "�������� ���� 'Deleted' ������ ������� �� �������� �� '0'");

            Assert.AreEqual(1, SQLs.Get_CurrentCategory(),
                "CurrentCategoryID � Settings �������� � ���������� '1' �� ���� [� �� ������� ����]");
        }
        #endregion

        #region Categories Removing

        public void Category_MarkAsRemoved(int categoryID, string categoryName)
        {
            SQLs.Add_NewCategory(categoryName);           //��������� ���� ������� (3-��)
            SQLs.TryAdd_Word_ToCategory(1, categoryID);   //��������� ������� ����� �� ���� �������
            SQLs.TryAdd_Word_ToCategory(2, categoryID);   //��������� ������� ����� �� ���� �������
            SQLs.TryMarkAsRemoved_Category(categoryID);   //���������� ������� �� "��������"
        }

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void RecentlyMarkedCategories_Removing_Test(string categoryName)
        {
            int categoryID = 3;
            Category_MarkAsRemoved(categoryID, categoryName);
            SQLs.FindAndRemove_LongMarkedCategories(3);

            using VocabularyContext db = new();

            //��������
            bool isRecentlyMarkedCategoryWordsDeleted = !db.WordCategories.Any(c => c.CategoryId == categoryID);
            Assert.IsFalse(isRecentlyMarkedCategoryWordsDeleted, 
                "����� ���� ������� ������� � WordCategory [� �� ������ ����]");

            bool isRecentlyMarkedCategoryDeleted = !db.Categories.Any(c => c.CategoryId == categoryID);
            Assert.IsFalse(isRecentlyMarkedCategoryDeleted, 
                "���� �������� �������� � Categories [� �� ������� ����]");
        }

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void LongMarkedCategories_Removing_Test(string categoryName)
        {
            int categoryID = 3;
            Category_MarkAsRemoved(categoryID, categoryName);

            using VocabularyContext db = new();

            //���� ���� (���������� ��) ��������� ���� �������
            db.Categories.First(c => c.CategoryId == categoryID).DeletedAt = new DateTime(2023, 01, 01);
            db.SaveChanges();
            SQLs.FindAndRemove_LongMarkedCategories(3);

            //��������
            bool isLongMarkedCategoryWordsDeleted = !db.WordCategories.Any(c => c.CategoryId == categoryID);
            Assert.IsTrue(isLongMarkedCategoryWordsDeleted,
                "����� ���� ������� �� ������� � WordCategory");

            bool isLongMarkedCategoryDeleted = !db.Categories.Any(c => c.CategoryId == categoryID);
            Assert.IsTrue(isLongMarkedCategoryDeleted,
                "���� �������� �� �������� � Categories");
        }
        #endregion

        #endregion

        #region [ WordCategories ]

        #region Adding words to category

        [TestCase(1)]
        [TestCase(2)]
        public void Word_Adding_ToCategory_Test(int wordID)
        {
            int anotherCategoryID = 2;

            using VocabularyContext db = new();

            //��������
            Assert.IsTrue(SQLs.TryAdd_Word_ToCategory(wordID, anotherCategoryID), 
                "����� �� ����� ����� �� ������� '2'");

            bool isAddedToCategory2 = db.WordCategories.Any(wc => wc.WordId == wordID &&  wc.CategoryId == anotherCategoryID);
            Assert.IsTrue(isAddedToCategory2, "����� �� �'������� � ������� '2'");
        }

        [TestCase(2, 2)]
        public void TwoIdenticalWords_Adding_ToCategory_Test(int anotherWordID, int anotherCategoryID)
        {
            Assert.IsTrue(SQLs.TryAdd_Word_ToCategory(anotherWordID, anotherCategoryID), 
                "����� �� ����� ����� �� ������� '2'");
            Assert.IsFalse(SQLs.TryAdd_Word_ToCategory(anotherWordID, anotherCategoryID), 
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

            TestDelegate AddValidWordIDToInvalidCategoryID = () => SQLs.TryAdd_Word_ToCategory(validWordID, invalidNumber);
            Assert.Catch(typeof(ArgumentException), AddValidWordIDToInvalidCategoryID,
                "��� ������� �������� ����������, � �� ������");

            TestDelegate AddInvalidWordIDToValidCategoryID = () => SQLs.TryAdd_Word_ToCategory(invalidNumber, validCategoryID);
            Assert.Catch(typeof(ArgumentException), AddInvalidWordIDToValidCategoryID,
                "��� ������� �������� ����������, � �� ������");

            TestDelegate AddInvalidWordIDToInvalidCategoryID = () => SQLs.TryAdd_Word_ToCategory(invalidNumber, invalidNumber);
            Assert.Catch(typeof(ArgumentException), AddInvalidWordIDToInvalidCategoryID,
                "��� ������� �������� ����������, � �� ������");
        }

        #endregion

        #region Removing words from category

        [TestCase(1)]
        [TestCase(2)]
        public void LastAddedWord_Removing_FromWordCategories_Test(int count)
        {
            int categoryID = 2;
            for (int i = 1; i <= count; i++)
                SQLs.TryAdd_Word_ToCategory(i, categoryID);    //��������� �����(-��) �� ���� �������
            SQLs.Remove_LastWords_FromCategory(count);         //��������� ����������(-��) �������� �� ���� ������� ����� � WordCategories

            using VocabularyContext db = new();

            int WordsCount = 2;
            for (int i = 1; i <= WordsCount; i++)
            {
                if (i <= count)
                {
                    bool isLastAddedWordRemovedFromWords = !db.WordCategories.Any(wc => wc.WordId == i && wc.CategoryId == 2);
                        //!DB.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = {i} AND CategoryID = 2").HasRows;
                    Assert.IsTrue(isLastAddedWordRemovedFromWords, "������ ����� ����� �� ���� �������");
                }
                else
                {
                    bool isAnotherWordNOTRemovedFromWords = db.WordCategories.Any(wc => wc.WordId == i && wc.CategoryId == 1);
                        //DB.Get_DataReader($"SELECT * FROM WordCategories WHERE WordID = {i} AND CategoryID = 1").HasRows;
                    Assert.IsTrue(isAnotherWordNOTRemovedFromWords, "���� ����� ���� ������� [� �� ������ ����]");
                }
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        public void Word_Removing_FromWordCategories_Test(int wordID)
        {
            int categoryID = 2;                                    //����� ��������� �������
            SQLs.TryAdd_Word_ToCategory(wordID, categoryID);       //��������� �����(-��) �� ���� �������

            Assert.Catch<ArgumentException>(() => SQLs.Remove_Word_FromCategory(wordID, 1), 
                "��� ������� �������� ����������, � �� ������");  //������ ��������� ����� � ������� �������

            using VocabularyContext db = new();

            SQLs.Remove_Word_FromCategory(wordID, categoryID);     //��������� ����� � ��������� �������
            bool isWordRemovedFromAnotherCategory = !db.WordCategories.Any(wc => wc.WordId == wordID && wc.CategoryId == 2);
            Assert.IsTrue(isWordRemovedFromAnotherCategory, "����� �� ���� �������� � ��������� �������");
        }

        #endregion

        #region Getting Words from Category

        /// <summary>
        /// ��������� ��� �� �� (� �������� � ��� �� ������ ������� '2')
        /// </summary>
        /// <param name="wordCount">ʳ������ ���</param>
        void Words_Adding_ToDB(int wordCount)
        {
            int anotherCategoryID = 2;
            int alreadyAddedWordsCount = 2;

            for (int i = 0; i < wordCount; i++)
                SQLs.TryAdd_Word_ToAllWords($"{i + 1}", "i + 1");
            for (int i = 0; i < wordCount + alreadyAddedWordsCount; i++)
                if (i % 2 == 0)
                    SQLs.TryAdd_Word_ToCategory(i + 1, anotherCategoryID);
        }

        [TestCase(4)]
        [TestCase(6)]
        public void Words_Getting_FromMainCategory_Test(int addedWordCount)
        {
            int getWordsCount = 4;
            int categoryID = 1;
            Words_Adding_ToDB(addedWordCount);

            var allWFromCategory = SQLs.Get_Words_FromCategory(categoryID);
            Assert.AreEqual(addedWordCount + 2, allWFromCategory.Count, 
                $"��������� ��� �� ���� {addedWordCount + 2} � �� {allWFromCategory.Count}");

            var countWFromCategory = SQLs.Get_Words_FromCategory(categoryID, getWordsCount);
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

            var allWFromCategory = SQLs.Get_Words_FromCategory(categoryID);
            //int addedWordCount
            Assert.AreEqual((int)Math.Round(((double)addedWordCount + 2)/2, MidpointRounding.AwayFromZero), allWFromCategory.Count,
                $"��������� ��� �� ���� {addedWordCount + 2} � �� {allWFromCategory.Count}");

            var countWFromCategory = SQLs.Get_Words_FromCategory(categoryID, getWordsCount);
            Assert.AreEqual(getWordsCount, countWFromCategory.Count,
                $"��������� ��� �� ���� {addedWordCount + 2} � �� {allWFromCategory.Count}");   
        }

        [TestCase(4)]
        [TestCase(6)]
        public void Words_NOTGetting_FromAnotherCategory_Test(int addedWordCount)
        {
            Words_Adding_ToDB(addedWordCount);
            int nonexistentWordCount = -2;
            TestDelegate GetWordsFromNonexistentCategory = () => SQLs.Get_Words_FromCategory(2, nonexistentWordCount);

            Assert.Catch(typeof(ArgumentException), GetWordsFromNonexistentCategory, 
                "��� ������� �������� ����������, � �� ������");
        }

        #endregion

        #endregion

        #region [ Settings ]

        #region Setting Current category

        [TestCase(2)]
        [TestCase(1)]
        public void CurrentCategory_SettingAndGetting_Test(int categoryID)
        {
            SQLs.Set_CurrentCategory(categoryID);        //������������ �������� CurrentCategoryID � Settings

            using VocabularyContext db = new(conStr);

            //��������
            var actualCurrentCategoryID = db.Settings.First(s => s.SettingsId == 1).CurrentCategoryId;
            Assert.AreEqual(categoryID, actualCurrentCategoryID,
                "CurrentCategoryID �� ����������� �� ���������� ���������");

            int gettedCurrentCategoryID = SQLs.Get_CurrentCategory();   //��������� �������� CurrentCategoryID � Settings
            int expectedCurrentCategoryID = actualCurrentCategoryID;    //���� ����������� �����
            Assert.AreEqual(expectedCurrentCategoryID, gettedCurrentCategoryID,
                "�������� �������� CurrentCategoryID - �� ����");
        }

        [Test]
        public void AllCategories_Getting_Test()
        {
            var categories = SQLs.Get_Categories();
            Assert.AreEqual("AllWords", categories[0].Name,
                "��������� ������ ���������� ��� ������� - �� �����");
        }
        #endregion

        #region Setting Number of Words to Learn

        [TestCase(1)]
        [TestCase(99)]
        public void NumberOfWordsToLearn_SettingAndGetting_Test(int count)
        {
            SQLs.SetNumberOfWordsToLearn(count);        //������������ �������� WordCountToLearn � Settings

            using VocabularyContext db = new(conStr);

            //��������
            var actualNumberOfWordsToLearn = db.Settings.First().WordCountToLearn;
            Assert.AreEqual(count, actualNumberOfWordsToLearn,
                "WordCountToLearn �� ����������� �� ���������� ���������");

            int gettedNumberOfWordsToLearn = SQLs.GetNumberOfWordsToLearn();   //��������� �������� WordCountToLearn � Settings
            int expectedNumberOfWordsToLearn = actualNumberOfWordsToLearn;     //���� ����������� �����
            Assert.AreEqual(expectedNumberOfWordsToLearn, gettedNumberOfWordsToLearn,
                "�������� �������� WordCountToLearn - �� ����");
        }

        [TestCase(0)]
        [TestCase(-12)]
        public void NumberOfWordsToLearn_NOTSettingBelowOne_Test(int count)
        {
            //������������ �������� CurrentCategoryID � Settings
            TestDelegate SetNumberOfWordsForFalseNumber = () => SQLs.SetNumberOfWordsToLearn(count);
            Assert.Catch(typeof(ArgumentException), SetNumberOfWordsForFalseNumber,
                "��� ������� �������� ����������, � �� ������");

            int gettedNumberOfWordsToLearn = SQLs.GetNumberOfWordsToLearn();   //��������� �������� WordCountToLearn � Settings
            Assert.AreNotEqual(count, gettedNumberOfWordsToLearn,
                "�������� �������� WordCountToLearn - �� ������� ���� ����������");
        }

        #endregion

        #region Setting Was Launched

        [TestCase(true)]
        [TestCase(false)]
        public void WasLaunched_SettingAndGetting_Test(bool wasLaunched)
        {
            SQLs.SetWasLaunched(wasLaunched);        //������������ �������� WasLaunched � Settings

            using VocabularyContext db = new(conStr);

            //��������
            var actualWasLaunched = db.Settings.First().WasLaunched;
            Assert.AreEqual(wasLaunched, actualWasLaunched,
                "WasLaunched �� ����������� �� ���������� ���������");

            var gettedWasLaunched = SQLs.WasLaunched();     //��������� �������� WasLaunched � Settings
            var expectedWasLaunched = actualWasLaunched;    //���� ����������� �����
            Assert.AreEqual(expectedWasLaunched, gettedWasLaunched,
                "�������� �������� WasLaunched - �� ����");
        }

        #endregion

        #region Setting Number of Words to Learn

        [TestCase(1)]
        [TestCase(99)]
        public void WordAddingMode_SettingAndGetting_Test(int count)
        {
            SQLs.SetWordAddingMode(count);        //������������ �������� WordAddingMode � Settings

            using VocabularyContext db = new(conStr);

            //��������
            var actualWordAddingMode = db.Settings.First().WordAddingMode;
            Assert.AreEqual(count, actualWordAddingMode,
                "WordAddingMode �� ����������� �� ���������� ���������");

            var gettedWordAddingMode = SQLs.GetWordAddingMode();   //��������� �������� WordAddingMode � Settings
            var expectedWordAddingMode = actualWordAddingMode;     //���� ����������� �����
            Assert.AreEqual(expectedWordAddingMode, gettedWordAddingMode,
                "�������� �������� WordAddingMode - �� ����");
        }

        [TestCase(-1)]
        [TestCase(-12)]
        public void WordAddingMode_NOTSettingBelowOne_Test(int count)
        {
            //������������ �������� WordAddingMode � Settings
            TestDelegate SetNonexistentWordAddingMode = () => SQLs.SetWordAddingMode(count);
            Assert.Catch(typeof(ArgumentException), SetNonexistentWordAddingMode,
                "��� ������� �������� ����������, � �� ������");

            int gettedWordAddingMode = SQLs.GetWordAddingMode();   //��������� �������� WordAddingMode � Settings
            Assert.AreNotEqual(count, gettedWordAddingMode,
                "�������� �������� WordCountToLearn - �� ������� ���� ����������");
        }

        #endregion

        #endregion


        //TODO - �������� ����� ...


        [TearDown]
        public void TearDown()
        {
            using VocabularyContext db = new();
            db.WordCategories.RemoveRange(db.WordCategories.Skip(2));
            db.AllWords.RemoveRange(db.AllWords.Skip(2));
            db.Categories.RemoveRange(db.Categories.Skip(2));
            db.Settings.First().WordCountToLearn = 5;
            db.SaveChanges();
        }
    }
}