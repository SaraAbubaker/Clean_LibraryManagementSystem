
namespace Library.Common.StringConstants
{
    public static class PermissionNames
    {
        public const string UserManage = "user.manage";
        public const string UserBasic = "user.basic";

        public const string UserTypeManage = "usertype.manage";
        public const string AuthManage = "auth.manage";

        public const string AuthorManage = "author.manage";

        public const string BookManage = "book.manage";
        public const string BookBasic = "book.basic";

        public const string BorrowManage = "borrow.manage";
        public const string BorrowBasic = "borrow.basic";

        public const string CategoryManage = "category.manage";
        public const string CategoryBasic = "category.basic";

        public const string InventoryManage = "inventory.manage";

        public const string PublisherManage = "publisher.manage";
        public const string PublisherBasic = "publisher.basic";

        public static readonly string[] All = new[]
        {
            UserManage, UserBasic,
            UserTypeManage, AuthManage,
            AuthorManage,
            BookManage, BookBasic,
            BorrowManage, BorrowBasic,
            CategoryManage, CategoryBasic,
            InventoryManage,
            PublisherManage, PublisherBasic
        };
    }
}