using EWL.EF_SQLite;
using System.Data.SQLite;
using NUnit.Framework;
using EWL;
using SQLitePCL;
using NUnit.Framework.Legacy;

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
    public class SQLs_Test
    {
        //�� ������� ��������� ����� � ��ò������� � �������Ҳ ����˲

        /// <summary>
        /// Connection String (contains path to .db file)
        /// </summary>
        readonly string conStr = "Data Source=D:\\SELF-DEV\\HARD-SKILLS\\DEVELOPMENT\\PRACTICE\\MyProjects\\EWL FC\\EWL_Tests\\Test_SQLs.db;";

        [OneTimeSetUp]
        public void Setup()
        {
            SQLService.CS = conStr;
            TearDown();
        }


        #region [ Words ]

        #region Words Adding

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void NewWord_Adding_Test(string engW, string uaW)
        {
            ClassicAssert.IsTrue(SQLService.TryAdd_Word_ToAllWords(engW, uaW),
                "����� �� ����� ������ ����� � Words");

            using VocabularyContext db = new(conStr);

            bool isAddedToWords = db.AllWords.Any(c => c.EngWord == engW && c.UaTranslation == uaW);
            ClassicAssert.IsTrue(isAddedToWords,
                "������ ����� �� ���� ������ � ��");

            bool isWordAddedToWordCategories = db.WordCategories.Any(c => c.WordId == 3);
            ClassicAssert.IsTrue(isWordAddedToWordCategories,
                "������ ����� �� ���� ������ �� ������� '1' � WordCategories");
        }

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void TwoIdenticalWords_Adding_Test(string engW, string uaW)
        {
            ClassicAssert.IsTrue(SQLService.TryAdd_Word_ToAllWords(engW, uaW),
                "����� �� ����� ������ ����� � Words");
            ClassicAssert.IsFalse(SQLService.TryAdd_Word_ToAllWords(engW, uaW),
                "����� ����� ������ ����� � Words [� �� ������� ���]");
        }

        #endregion

        #region Words Removing

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void LastWords_Removing_Test(string engW, string uaW)
        {
            var (anotherWId, anotherEngW, AnotherUaW) = (3, "candle", "�����");
            SQLService.TryAdd_Word_ToAllWords(anotherEngW, AnotherUaW);   //��������� ������� �����
            SQLService.TryAdd_Word_ToAllWords(engW, uaW);                 //��������� ������� �����
            SQLService.Remove_LastWords_Permanently(1);                   //��������� ���������� �����

            using VocabularyContext db = new(conStr);

            //��������
            bool isLastAddedWordRemovedFromWords = !db.AllWords.Any(c => c.EngWord == engW);
            ClassicAssert.IsTrue(isLastAddedWordRemovedFromWords, "������ ����� �� ���� ��������");

            bool isAnotherWordNOTRemovedFromWords = db.AllWords.Any(c => c.EngWord == anotherEngW);
            ClassicAssert.IsTrue(isAnotherWordNOTRemovedFromWords, "���� ����� ���� �������� [� �� ������� ����]");

            bool isLastAddedWordRemovedFromWordCategories = !db.WordCategories.Any(c => c.WordId == 4);
            ClassicAssert.IsTrue(isLastAddedWordRemovedFromWordCategories, "������ ����� �� ���� �������� � WordCategories");

            bool isAnotherWordRemovedFromWordCategories = db.WordCategories.Any(c => c.WordId == anotherWId);
            ClassicAssert.IsTrue(isAnotherWordRemovedFromWordCategories, "���� ����� ���� �������� � WordCategories [� �� ������� ����]");
        }

        [TestCaseSource(typeof(MyTestCases), "Words_Cases")]
        public void Word_Removing_Test(string engW, string uaW)
        {
            var (anotherWId, anotherEngW, AnotherUaW) = (3, "candle", "�����");
            SQLService.TryAdd_Word_ToAllWords(anotherEngW, AnotherUaW);   //��������� ������� �����
            SQLService.TryAdd_Word_ToAllWords(engW, uaW);                 //��������� ������� �����
            SQLService.Remove_Word_Permanently(4);                        //��������� ������� �����

            using VocabularyContext db = new(conStr);

            //��������
            bool isLastAddedWordRemovedFromWords = !db.AllWords.Any(c => c.EngWord == engW);
            ClassicAssert.IsTrue(isLastAddedWordRemovedFromWords, "����� ����� �� ���� ��������");

            bool isAnotherWordNOTRemovedFromWords = db.AllWords.Any(c => c.EngWord == anotherEngW);
            ClassicAssert.IsTrue(isAnotherWordNOTRemovedFromWords, "����� ����� ���� �������� [� �� ������� ����]");

            bool isLastAddedWordRemovedFromWordCategories = !db.WordCategories.Any(c => c.WordId == 4);
            ClassicAssert.IsTrue(isLastAddedWordRemovedFromWordCategories, "����� ����� �� ���� �������� � WordCategories");

            bool isAnotherWordRemovedFromWordCategories = db.WordCategories.Any(c => c.WordId == anotherWId);
            ClassicAssert.IsTrue(isAnotherWordRemovedFromWordCategories, "����� ����� ���� �������� � WordCategories [� �� ������� ����]");
        }

        #endregion

        #region Changing word parametrs

        [TestCase(1, 4)]
        [TestCase(1, 0)]
        public void WordRating_Setting_Test(int wordId, int rating)
        {
            SQLService.Rate_Word(wordId, rating);        //������������ �������� Rating � Word

            using VocabularyContext db = new(conStr);

            var actualRating = db.AllWords.First(w => w.WordId == wordId).Rating;
            ClassicAssert.AreEqual(rating, actualRating,
                "WordAddingMode �� ����������� �� ���������� ���������");
        }

        [TestCase(0, 4)]
        [TestCase(1, -1)]
        public void WordRating_NOTSettingBelowOne_Test(int wordId, int rating)
        {
            //������������ �������� Rating � Word
            ClassicAssert.Catch(typeof(ArgumentException), () => SQLService.Rate_Word(wordId, rating),
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
                SQLService.Increment_WordRepetition(wordId);        //��������� �������� Repetition � Word
            }
            using (VocabularyContext db = new(conStr))
            {
                var actualRepetition = db.AllWords.First(w => w.WordId == wordId).Repetition;
                ClassicAssert.AreEqual(repetition + 1, actualRepetition,
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
            ClassicAssert.IsTrue(SQLService.Add_NewCategory(categoryName),
                "��������� ���� ������� �� �����");

            using VocabularyContext db = new(conStr);

            bool isCategoryAdded = db.Categories.Any(c => c.Name == categoryName);
            ClassicAssert.IsTrue(isCategoryAdded,
                "���� �������� �� �'������� � Categories");
        }

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void TwoIdenticalCategories_Adding_Test(string categoryName)
        {
            ClassicAssert.IsTrue(SQLService.Add_NewCategory(categoryName),
                "��������� ���� ������� �� �����");
            ClassicAssert.IsFalse(SQLService.Add_NewCategory(categoryName),
                "��������� ���� ������� ����� [� �� ������� ����]");
        }

        #endregion

        #region Categories Marking as Removed

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void Category_MarkingAsRemoved_Test(string categoryName)
        {
            int categoryID = 3;
            SQLService.Add_NewCategory(categoryName);          //��������� ���� ������� (3-��)
            SQLService.Set_CurrentCategory(categoryID);        //������������ ������� ������� - '3'

            using VocabularyContext db = new(conStr);

            //��������
            ClassicAssert.IsTrue(SQLService.TryMarkAsRemoved_Category(categoryID),
            "������ �������� �� ���� (��������� ��) ��������");

            var category = db.Categories.First(c => c.CategoryId == categoryID);
            bool categoryIsMarkedAsDeleted = category.Deleted;
            ClassicAssert.IsTrue(categoryIsMarkedAsDeleted,
                "�������� ���� 'Deleted' ������ ������� �� �������� �� '1'");

            ClassicAssert.AreEqual(1, db.Settings.First().CurrentCategoryId,
                "CurrentCategoryID � Settings �� �������� �� �������� '1'");

            DateTime actualDateTime = category.DeletedAt;
            ClassicAssert.IsTrue(($"{actualDateTime:yyyy-MM-dd HH:mm}" == $"{DateTime.Now:yyyy-MM-dd HH:mm}"),
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
            ClassicAssert.IsFalse(SQLService.TryMarkAsRemoved_Category(categoryID),
                "���� �������� ���� (��������� ��) �������� [� �� ������� ����]");

            bool categoryIsMarkedAsDeleted = db.Categories.First(c => c.CategoryId == categoryID).Deleted;
            ClassicAssert.IsFalse(categoryIsMarkedAsDeleted,
                "�������� ���� 'Deleted' ������ ������� �������� �� '1' [� �� ������� ����]");
        }
        #endregion

        #region Categories Restoring from Deletion

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void Category_Restoring_FromDeletion_Test(string categoryName)
        {
            int categoryID = 3;
            SQLService.Add_NewCategory(categoryName);           //��������� ���� ������� (3-��)
            SQLService.Set_CurrentCategory(categoryID);         //������������ ������� ������� - '3'
            SQLService.TryMarkAsRemoved_Category(categoryID);   //���������� ������� �� "��������"

            SQLService.Restore_Category_FromDeletion(categoryID);   //³��������� ������� � "������"

            using VocabularyContext db = new(conStr);

            bool categoryIsMarkedAsDeleted = db.Categories.First(c => c.CategoryId == categoryID).Deleted;
            ClassicAssert.IsFalse(categoryIsMarkedAsDeleted,
                "�������� ���� 'Deleted' ������ ������� �� �������� �� '0'");

            ClassicAssert.AreEqual(1, SQLService.Get_CurrentCategory(),
                "CurrentCategoryID � Settings �������� � ���������� '1' �� ���� [� �� ������� ����]");
        }
        #endregion

        #region Categories Removing

        public void Category_MarkAsRemoved(int categoryID, string categoryName)
        {
            SQLService.Add_NewCategory(categoryName);           //��������� ���� ������� (3-��)
            SQLService.TryAdd_Word_ToCategory(1, categoryID);   //��������� ������� ����� �� ���� �������
            SQLService.TryAdd_Word_ToCategory(2, categoryID);   //��������� ������� ����� �� ���� �������
            SQLService.TryMarkAsRemoved_Category(categoryID);   //���������� ������� �� "��������"
        }

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void RecentlyMarkedCategories_Removing_Test(string categoryName)
        {
            int categoryID = 3;
            Category_MarkAsRemoved(categoryID, categoryName);
            SQLService.FindAndRemove_LongMarkedCategories(3);

            using VocabularyContext db = new();

            //��������
            bool isRecentlyMarkedCategoryWordsDeleted = !db.WordCategories.Any(c => c.CategoryId == categoryID);
            ClassicAssert.IsFalse(isRecentlyMarkedCategoryWordsDeleted, 
                "����� ���� ������� ������� � WordCategory [� �� ������ ����]");

            bool isRecentlyMarkedCategoryDeleted = !db.Categories.Any(c => c.CategoryId == categoryID);
            ClassicAssert.IsFalse(isRecentlyMarkedCategoryDeleted, 
                "���� �������� �������� � Categories [� �� ������� ����]");
        }

        [TestCaseSource(typeof(MyTestCases), "Categories_Cases")]
        public void LongMarkedCategories_Removing_Test(string categoryName)
        {
            int categoryID = 3;
            Category_MarkAsRemoved(categoryID, categoryName);

            using VocabularyContext db = new();

            //���� ���� (���������� ��) ��������� ���� �������
            db.Categories.First(c => c.CategoryId == categoryID).DeletedAt = 
                new DateTime(year: 2023, month: 01, day: 01, kind: DateTimeKind.Utc, hour: 12, minute: 0, second: 0);
            db.SaveChanges();
            SQLService.FindAndRemove_LongMarkedCategories(3);

            //��������
            bool isLongMarkedCategoryWordsDeleted = !db.WordCategories.Any(c => c.CategoryId == categoryID);
            ClassicAssert.IsTrue(isLongMarkedCategoryWordsDeleted,
                "����� ���� ������� �� ������� � WordCategory");

            bool isLongMarkedCategoryDeleted = !db.Categories.Any(c => c.CategoryId == categoryID);
            ClassicAssert.IsTrue(isLongMarkedCategoryDeleted,
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
            ClassicAssert.IsTrue(SQLService.TryAdd_Word_ToCategory(wordID, anotherCategoryID), 
                "����� �� ����� ����� �� ������� '2'");

            bool isAddedToCategory2 = db.WordCategories.Any(wc => wc.WordId == wordID &&  wc.CategoryId == anotherCategoryID);
            ClassicAssert.IsTrue(isAddedToCategory2, "����� �� �'������� � ������� '2'");
        }

        [TestCase(2, 2)]
        public void TwoIdenticalWords_Adding_ToCategory_Test(int anotherWordID, int anotherCategoryID)
        {
            ClassicAssert.IsTrue(SQLService.TryAdd_Word_ToCategory(anotherWordID, anotherCategoryID), 
                "����� �� ����� ����� �� ������� '2'");
            ClassicAssert.IsFalse(SQLService.TryAdd_Word_ToCategory(anotherWordID, anotherCategoryID), 
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

            TestDelegate AddValidWordIDToInvalidCategoryID = () => SQLService.TryAdd_Word_ToCategory(validWordID, invalidNumber);
            ClassicAssert.Catch(typeof(ArgumentException), AddValidWordIDToInvalidCategoryID,
                "��� ������� �������� ����������, � �� ������");

            TestDelegate AddInvalidWordIDToValidCategoryID = () => SQLService.TryAdd_Word_ToCategory(invalidNumber, validCategoryID);
            ClassicAssert.Catch(typeof(ArgumentException), AddInvalidWordIDToValidCategoryID,
                "��� ������� �������� ����������, � �� ������");

            TestDelegate AddInvalidWordIDToInvalidCategoryID = () => SQLService.TryAdd_Word_ToCategory(invalidNumber, invalidNumber);
            ClassicAssert.Catch(typeof(ArgumentException), AddInvalidWordIDToInvalidCategoryID,
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
                SQLService.TryAdd_Word_ToCategory(i, categoryID);    //��������� �����(-��) �� ���� �������
            SQLService.Remove_LastWords_FromCategory(count);         //��������� ����������(-��) �������� �� ���� ������� ����� � WordCategories

            using VocabularyContext db = new();

            int WordsCount = 2;
            for (int i = 1; i <= WordsCount; i++)
            {
                if (i <= count)
                {
                    bool isLastAddedWordRemovedFromWords = !db.WordCategories.Any(wc => wc.WordId == i && wc.CategoryId == 2);
                    ClassicAssert.IsTrue(isLastAddedWordRemovedFromWords, "������ ����� ����� �� ���� �������");
                }
                else
                {
                    bool isAnotherWordNOTRemovedFromWords = db.WordCategories.Any(wc => wc.WordId == i && wc.CategoryId == 1);
                    ClassicAssert.IsTrue(isAnotherWordNOTRemovedFromWords, "���� ����� ���� ������� [� �� ������ ����]");
                }
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        public void Word_Removing_FromWordCategories_Test(int wordID)
        {
            int categoryID = 2;                                    //����� ��������� �������
            SQLService.TryAdd_Word_ToCategory(wordID, categoryID);       //��������� �����(-��) �� ���� �������

            ClassicAssert.Catch<ArgumentException>(() => SQLService.Remove_Word_FromCategory(wordID, 1), 
                "��� ������� �������� ����������, � �� ������");  //������ ��������� ����� � ������� �������

            using VocabularyContext db = new();

            SQLService.Remove_Word_FromCategory(wordID, categoryID);     //��������� ����� � ��������� �������
            bool isWordRemovedFromAnotherCategory = !db.WordCategories.Any(wc => wc.WordId == wordID && wc.CategoryId == 2);
            ClassicAssert.IsTrue(isWordRemovedFromAnotherCategory, "����� �� ���� �������� � ��������� �������");
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
                SQLService.TryAdd_Word_ToAllWords($"{i + 1}", "i + 1");
            for (int i = 0; i < wordCount + alreadyAddedWordsCount; i++)
                if (i % 2 == 0)
                    SQLService.TryAdd_Word_ToCategory(i + 1, anotherCategoryID);
        }

        [TestCase(4)]
        [TestCase(6)]
        public void Words_Getting_FromMainCategory_Test(int addedWordCount)
        {
            int getWordsCount = 4;
            int categoryID = 1;
            Words_Adding_ToDB(addedWordCount);

            var allWFromCategory = SQLService.Get_Words_FromCategory(categoryID);
            ClassicAssert.AreEqual(addedWordCount + 2, allWFromCategory.Count, 
                $"��������� ��� �� ���� {addedWordCount + 2} � �� {allWFromCategory.Count}");

            var countWFromCategory = SQLService.Get_Words_FromCategory(categoryID, getWordsCount);
            ClassicAssert.AreEqual(getWordsCount, countWFromCategory.Count, 
                $"��������� ��� �� ���� {getWordsCount} � �� {allWFromCategory.Count}");
        }

        [TestCase(4)]
        [TestCase(6)]
        public void Words_Getting_FromAnotherCategory_Test(int addedWordCount)
        {
            int getWordsCount = 2;
            int categoryID = 2;
            Words_Adding_ToDB(addedWordCount);

            var allWFromCategory = SQLService.Get_Words_FromCategory(categoryID);
            //int addedWordCount
            ClassicAssert.AreEqual((int)Math.Round(((double)addedWordCount + 2)/2, MidpointRounding.AwayFromZero), allWFromCategory.Count,
                $"��������� ��� �� ���� {addedWordCount + 2} � �� {allWFromCategory.Count}");

            var countWFromCategory = SQLService.Get_Words_FromCategory(categoryID, getWordsCount);
            ClassicAssert.AreEqual(getWordsCount, countWFromCategory.Count,
                $"��������� ��� �� ���� {addedWordCount + 2} � �� {allWFromCategory.Count}");   
        }

        [TestCase(4)]
        [TestCase(6)]
        public void Words_NOTGetting_FromAnotherCategory_Test(int addedWordCount)
        {
            Words_Adding_ToDB(addedWordCount);
            int nonexistentWordCount = -2;
            TestDelegate GetWordsFromNonexistentCategory = () => SQLService.Get_Words_FromCategory(2, nonexistentWordCount);

            ClassicAssert.Catch(typeof(ArgumentException), GetWordsFromNonexistentCategory, 
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
            SQLService.Set_CurrentCategory(categoryID);        //������������ �������� CurrentCategoryID � Settings

            using VocabularyContext db = new(conStr);

            //��������
            var actualCurrentCategoryID = db.Settings.First(s => s.SettingsId == 1).CurrentCategoryId;
            ClassicAssert.AreEqual(categoryID, actualCurrentCategoryID,
                "CurrentCategoryID �� ����������� �� ���������� ���������");

            int gettedCurrentCategoryID = SQLService.Get_CurrentCategory();   //��������� �������� CurrentCategoryID � Settings
            int expectedCurrentCategoryID = actualCurrentCategoryID;    //���� ����������� �����
            ClassicAssert.AreEqual(expectedCurrentCategoryID, gettedCurrentCategoryID,
                "�������� �������� CurrentCategoryID - �� ����");
        }

        [Test]
        public void AllCategories_Getting_Test()
        {
            var categories = SQLService.Get_Categories();
            ClassicAssert.AreEqual("AllWords", categories[0].Name,
                "��������� ������ ���������� ��� ������� - �� �����");
        }
        #endregion

        #region Setting Number of Words to Learn

        [TestCase(1)]
        [TestCase(99)]
        public void NumberOfWordsToLearn_SettingAndGetting_Test(int count)
        {
            SQLService.Set_NumberOfWordsToLearn(count);        //������������ �������� WordCountToLearn � Settings

            using VocabularyContext db = new(conStr);

            //��������
            var actualNumberOfWordsToLearn = db.Settings.First().WordCountToLearn;
            ClassicAssert.AreEqual(count, actualNumberOfWordsToLearn,
                "WordCountToLearn �� ����������� �� ���������� ���������");

            int gettedNumberOfWordsToLearn = SQLService.Get_NumberOfWordsToLearn();   //��������� �������� WordCountToLearn � Settings
            int expectedNumberOfWordsToLearn = actualNumberOfWordsToLearn;     //���� ����������� �����
            ClassicAssert.AreEqual(expectedNumberOfWordsToLearn, gettedNumberOfWordsToLearn,
                "�������� �������� WordCountToLearn - �� ����");
        }

        [TestCase(0)]
        [TestCase(-12)]
        public void NumberOfWordsToLearn_NOTSettingBelowOne_Test(int count)
        {
            //������������ �������� CurrentCategoryID � Settings
            TestDelegate SetNumberOfWordsForFalseNumber = () => SQLService.Set_NumberOfWordsToLearn(count);
            ClassicAssert.Catch(typeof(ArgumentException), SetNumberOfWordsForFalseNumber,
                "��� ������� �������� ����������, � �� ������");

            int gettedNumberOfWordsToLearn = SQLService.Get_NumberOfWordsToLearn();   //��������� �������� WordCountToLearn � Settings
            ClassicAssert.AreNotEqual(count, gettedNumberOfWordsToLearn,
                "�������� �������� WordCountToLearn - �� ������� ���� ����������");
        }

        #endregion

        #region Setting Was Launched

        [TestCase(true)]
        [TestCase(false)]
        public void WasLaunched_SettingAndGetting_Test(bool wasLaunched)
        {
            SQLService.Set_WasLaunched(wasLaunched);        //������������ �������� WasLaunched � Settings

            using VocabularyContext db = new(conStr);

            //��������
            var actualWasLaunched = db.Settings.First().WasLaunched;
            ClassicAssert.AreEqual(wasLaunched, actualWasLaunched,
                "WasLaunched �� ����������� �� ���������� ���������");

            var gettedWasLaunched = SQLService.WasLaunched();     //��������� �������� WasLaunched � Settings
            var expectedWasLaunched = actualWasLaunched;    //���� ����������� �����
            ClassicAssert.AreEqual(expectedWasLaunched, gettedWasLaunched,
                "�������� �������� WasLaunched - �� ����");
        }

        #endregion

        #region Setting Number of Words to Learn

        [TestCase(1)]
        [TestCase(99)]
        public void WordAddingMode_SettingAndGetting_Test(int count)
        {
            SQLService.Set_WordAddingMode(count);        //������������ �������� WordAddingMode � Settings

            using VocabularyContext db = new(conStr);

            //��������
            var actualWordAddingMode = db.Settings.First().WordAddingMode;
            ClassicAssert.AreEqual(count, actualWordAddingMode,
                "WordAddingMode �� ����������� �� ���������� ���������");

            var gettedWordAddingMode = SQLService.Get_WordAddingMode();   //��������� �������� WordAddingMode � Settings
            var expectedWordAddingMode = actualWordAddingMode;     //���� ����������� �����
            ClassicAssert.AreEqual(expectedWordAddingMode, gettedWordAddingMode,
                "�������� �������� WordAddingMode - �� ����");
        }

        [TestCase(-1)]
        [TestCase(-12)]
        public void WordAddingMode_NOTSettingBelowOne_Test(int count)
        {
            //������������ �������� WordAddingMode � Settings
            TestDelegate SetNonexistentWordAddingMode = () => SQLService.Set_WordAddingMode(count);
            ClassicAssert.Catch(typeof(ArgumentException), SetNonexistentWordAddingMode,
                "��� ������� �������� ����������, � �� ������");

            int gettedWordAddingMode = SQLService.Get_WordAddingMode();   //��������� �������� WordAddingMode � Settings
            ClassicAssert.AreNotEqual(count, gettedWordAddingMode,
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