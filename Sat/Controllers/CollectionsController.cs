using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Task4Core.Models;
using Task4Core.ViewModels;
using System.Data.Entity;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Task4Core.Controllers
{
    
    public class CollectionsController : Controller
    {
        IConfiguration _configuration;
        AppDbContext _appDBContext;
        public MarkdownSharp.Markdown markdown = new();
        IEnumerable<Collection> collections;
       
        public CollectionsController(IConfiguration configuration, AppDbContext appDBContext)

        {
            _appDBContext = appDBContext;
            _configuration = configuration;
        }

        public IActionResult Index(string Topic,string sortOrder,string searchString)
        {

            FetchData(Topic);
            ViewBag.NameSortParm = sortOrder=="Name" ? "name_desc" : "Name";           
            ViewBag.LikeSortParm = sortOrder == "Like" ? "like_desc" : "Like";
            ViewBag.ItemSortParm = sortOrder == "Item" ? "item_desc" : "Item";
            ViewData["CurrentFilter"] = searchString;
            SortCollections(sortOrder);
            collections = searchString==null?collections: SearchCollection(searchString);

            return View(collections.ToList());

        }

        
        private void SortCollections(string sortOrder)
        {
            switch (sortOrder)
            {
                case "name_desc":
                    collections = collections.OrderByDescending(s => s.CollectionName);
                    break;
                case "Name":
                    collections = collections.OrderBy(s => s.CollectionName);
                    break;
                case "Like":
                    collections = collections.OrderBy(s => s.LikeCount);
                    break;
                case "like_desc":
                    collections = collections.OrderByDescending(s => s.LikeCount);
                    break;
                case "item_desc":
                    collections = collections.OrderByDescending(s => s.ItemCount);
                    break;
                case "Item":
                    collections = collections.OrderBy(s => s.ItemCount);
                    break;
                default:
                    collections = collections.OrderBy(s => s.ID);
                    break;
            }
        }

        private void FetchData(string collection)
        {
            if (collection != null)
                collections = _appDBContext.Collections.Where(i => i.Topic == collection);
            else
                collections = _appDBContext.Collections;
        }
        public IActionResult AddItem(int CollectionId)
        {
         
            return View(new AddItemViewModel() {Id=_appDBContext.Collections.Where(s=>s.ID==CollectionId).First() });
        }
        [HttpPost]
        public IActionResult AddItem(AddItemViewModel model,int CollectionId,IEnumerable<string> Tags)
        {                           
            if (ModelState.IsValid)
            {     
                _appDBContext.Items.Add(new Item
                {
                    NameItem = model.ItemName,
                    IdCollection = CollectionId,
                    FirstField_Int = model.FirstFiled_Int,
                    FirstField_String = model.FirstFiled_String,
                    FirstField_Bool = model.FirstFiled_Bool,
                    SecondField_Int = model.SecondFiled_Int,
                    SecondField_Bool = model.SecondFiled_Bool,
                    SecondField_String = model.SecondFiled_String,
                    ThirdField_Int=model.ThirdFiled_Int,
                    ThirdField_Bool=model.ThirdFiled_Bool,
                    ThirdField_String=model.ThirdFiled_String,
                    FirstField_Data = model.FirstFiled_Data!=DateTime.MinValue?model.FirstFiled_Data:DateTime.Now,
                    SecondField_Data = model.SecondFiled_Data != DateTime.MinValue ? model.SecondFiled_Data : DateTime.Now,
                    ThirdField_Data = model.ThirdFiled_Data != DateTime.MinValue ? model.ThirdFiled_Data : DateTime.Now,
                    Tags = TagsToString(Tags)
                });
                AddTags(Tags);
               
                return RedirectToAction("MyColl",new { Username= AddItemCount(CollectionId).UserName});
            }         
            return View(SetCollectionsInModel(model,CollectionId));
        }

        private Collection AddItemCount(int id)
        {
            var CollectionInfo = _appDBContext.Collections.Where(i => i.ID == id).First();
            CollectionInfo.ItemCount++;
            _appDBContext.Collections.Update(CollectionInfo);
            _appDBContext.SaveChanges();
            return CollectionInfo;
        }

        private AddItemViewModel SetCollectionsInModel (AddItemViewModel model,int CollectionId)
        {
            var CollectionInfo = _appDBContext.Collections.Where(i => i.ID == CollectionId).First();
            return new AddItemViewModel() { Id = CollectionInfo, FirstList = Parse(CollectionInfo.FirsList), SecondList = Parse(CollectionInfo.SecondList), ThirdList = Parse(CollectionInfo.ThirdList) }; 
        }
        private string TagsToString(IEnumerable<string> Tags)
        {
            string tagsString = String.Empty;
            foreach (var s in Tags)
            {
                tagsString += '#' + s;
            }
            return tagsString;
        } 
        public List<string> Parse(string str)
        {
            return str?.Split(",").ToList();
        }
        private void AddTags(IEnumerable<string> Tags)
        {
            foreach(var tag in Tags)
            {
                if (_appDBContext.Tags.Where(s => s.Tag == tag).Count() == 0)
                {
                    _appDBContext.Tags.Add(new Tags() { Tag=tag});                    
                }
            }
            _appDBContext.SaveChanges();
        }
        public IActionResult AddCollection(string Username)
        {
            var UserName = Username ?? User.Identity.Name;
            return View(new AddCollectionsViewModel() {UserName= UserName });
        }
        [HttpPost]
        public IActionResult AddCollection(AddCollectionsViewModel model,IEnumerable<string> list,IEnumerable<string> FieldName, IEnumerable<string> FieldListName, string Username)
        {
            var UserName = Username ?? User.Identity.Name;
            model.UserName = UserName;
            if (ModelState.IsValid){
                List<string> type = list.ToList();
                List<string> name = FieldName.ToList();
                List<string> listField = FieldListName.ToList();
                _appDBContext.Collections.Add(new Collection { CollectionName = model.CollectionsName, 
                    Topic = model.CollectionsTopic,
                    UserName = UserName,
                    Description=model.Description,
                    FirstField=type.Count>=1?type[0]:null,
                    SecondFiled= type.Count >= 2 ? type[1] : null,
                    ThirdFiled= type.Count >= 3 ? type[2] : null,
                    FirstFieldName=name.Count>=1?name[0]:null,
                    SecondFieldName = name.Count >= 2 ? name[1] : null,
                    ThirdFieldName = name.Count >= 3 ? name[2] : null,
                    FirsList = listField.Count >= 1 ? listField[0] : null,
                    SecondList = listField.Count >= 2 ? listField[1] : null,
                    ThirdList = listField.Count >= 3 ? listField[2] : null,
                    LikeCount =0,ItemCount=0});
                _appDBContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }
        public IActionResult MyColl(string Username)
        {
            var UserName = Username ?? User.Identity.Name;
            return View(new UserAndCollections() { collections = (_appDBContext.Collections.Where(i => i.UserName == UserName).ToList()), UserName = UserName });
        }
        
        public IActionResult DeleteCollections(int id)
        {
           var coll = _appDBContext.Collections.Where(s => s.ID == id).First();
           var items = _appDBContext.Items.Where(s => s.IdCollection == coll.ID).ToList();
           foreach(var it in items)
           {
               DeleteItem(it.IDItem);
           }
            _appDBContext.Collections.Remove(coll);
            _appDBContext.SaveChanges();
            return RedirectToAction("MyColl", new {Username=coll.UserName });
        }
        
        public IActionResult DeleteItem(int id)
        {
            var item = _appDBContext.Items.Where(s => s.IDItem == id).First();
            DeleteComments(id);
            _appDBContext.Items.Remove(item);
            _appDBContext.SaveChanges();
            return RedirectToAction("CollectionPage", new { CollectionId = RemoveComAndLike(GetLikesCount(item.Likes),item.IdCollection).ID });
        }
        private Collection RemoveComAndLike(int LikeCount,int IdCollection)
        {
            var col = _appDBContext.Collections.Where(s => s.ID == IdCollection).First();
            col.LikeCount -= LikeCount;
            col.ItemCount--;
            _appDBContext.Collections.Update(col);
            _appDBContext.SaveChanges();
            return col;
        }
        public void DeleteComments(int id)
        {
            var comments = _appDBContext.Comments.Where(s => s.ItemId == id).ToList();
            foreach(var com in comments) {
                _appDBContext.Comments.Remove(com);
            }
        }
        public IActionResult CollectionPage(int CollectionId, string sortOrder, string searchString)
        {
            var Items = _appDBContext.Items.Where(i => i.IdCollection == CollectionId);           
            var collection = _appDBContext.Collections.Where(I => I.ID == CollectionId).ToList().First();
            var markdown = new MarkdownSharp.Markdown();
            collection.Description = markdown.Transform(collection.Description);
            ViewBag.NameSortParm = sortOrder == "Name" ? "name_desc" : "Name";
            ViewBag.DateSortParm = sortOrder == "Like" ? "like_sedc" : "Like";
            ViewBag.CommentsSortParm = sortOrder == "Comments" ? "comments_desc" : "Comments";
            Items = SearchItem(searchString, Items);
            Items = SortItems(sortOrder,Items);
           return View(new CollectionsItems() { collections = collection, items = Items.ToList() });
        }

        private IQueryable<Item> SortItems(string sortOrder, IQueryable<Item> Items)
        {
            switch (sortOrder)
            {
                case "name_desc":
                    Items = Items.OrderByDescending(s => s.NameItem);
                    break;
                case "Like":
                    Items = Items.OrderBy(s => s.LikesCount);
                    break;
                case "Name":
                    Items = Items.OrderBy(s => s.NameItem);
                    break;
                case "like_sedc":
                    Items = Items.OrderByDescending(s => s.LikesCount);
                    break;
                case "Comments":
                    Items = Items.OrderBy(s => s.CommentsCount);
                    break;
                case "comments_desc":
                    Items = Items.OrderByDescending(s => s.CommentsCount);
                    break;
                default:
                    Items = Items.OrderBy(s => s.IDItem);
                    break;
            }
            return Items;
        }
        private IQueryable<Item>  SearchItem(string searchString, IQueryable<Item> Items)
        {
            if (!String.IsNullOrEmpty(searchString))
            {
                Items = Items.Where(s => s.NameItem.Contains(searchString));
            }
            return Items;
        }
        public IActionResult Items(string searchString)
        {
           
            if (!String.IsNullOrEmpty(searchString))
            {          
                List<Item> items = new();
                AddItemsToListFromCollection(ref items, SearchCollection(searchString).ToList());              
                AddItemsToListFromComments(ref items, new HashSet<int>(SearchComments(searchString)));
                items.AddRange(SearchItem(searchString).ToList());
                var unique_items = new HashSet<Item>(items);
                return View(unique_items.ToList());
            } 
            return RedirectToAction("Index");
        }

        private void AddItemsToListFromCollection( ref List<Item> items,List<Collection> Listcoll)
        {
            foreach (var col in Listcoll)
            {
                var list = _appDBContext.Items.Where(s => s.IdCollection == col.ID);
                items.AddRange(list);
            }
           
        }
        private void AddItemsToListFromComments( ref List<Item> items, HashSet<int> ListIdComm )
        {
            foreach (var id in ListIdComm)
            {
                var list = _appDBContext.Items.Where(s => s.IDItem == id);
                items.AddRange(list);
            }
           
        }

        private List<Item> AddToList(List<Item> items, List<Collection> Listcoll)
        {
            foreach (var col in Listcoll)
            {
                var list = _appDBContext.Items.Where(s => s.IdCollection == col.ID);
                items.AddRange(list);
            }
            return items;
        }


        private IQueryable<Item> SearchItem(string searchString)
        {
            var results = from p in _appDBContext.Items
                          where EF.Functions.Contains(p.NameItem, searchString) || EF.Functions.Contains(p.Tags, searchString) || EF.Functions.Contains(p.FirstField_String, searchString) || EF.Functions.Contains(p.SecondField_String, searchString) || EF.Functions.Contains(p.ThirdField_String, searchString)
                          select p;
            return results;
        }
        private IQueryable<Collection> SearchCollection(string searchString)
        {
            var results = from p in _appDBContext.Collections
                          where EF.Functions.Contains(p.Description, searchString)|| EF.Functions.Contains(p.CollectionName, searchString)
                          select p;
            return results;
        }
        private IQueryable<int> SearchComments(string searchString)
        {
            var comments = from p in _appDBContext.Comments
                           where EF.Functions.Contains(p.Comment, searchString)
                           select p.ItemId;
            return comments;
        }
        public IActionResult ItemPage(int ItemId)
        {
            var res = _appDBContext.Items.Where(s => s.IDItem == ItemId).First();
            var markdown = new MarkdownSharp.Markdown();
            res.FirstField_String = markdown.Transform(res.FirstField_String);
            res.SecondField_String = markdown.Transform(res.SecondField_String);
            res.ThirdField_String = markdown.Transform(res.ThirdField_String);
            var col = _appDBContext.Collections.Where(s => s.ID == res.IdCollection).First();
            return View(new ItemAndCollection() { collection = col, item = res,LikesCount=GetLikesCount(res.Likes) });
        }

        public int GetLikesCount(string likeString)
        {
            return likeString!=null?(likeString.Length - likeString.Replace(",", "").Length):0;
        }
        public IActionResult AddLike(int ItemId)
        {
            Item item = _appDBContext.Items.Where(s => s.IDItem == ItemId).First();
            Collection collection = _appDBContext.Collections.Where(s => s.ID == item.IdCollection).FirstOrDefault();
            item.Likes +=User.Identity.Name+",";
            item.LikesCount++;
            _appDBContext.Items.Update(item);
            collection.LikeCount++;
            _appDBContext.Collections.Update(collection);
            _appDBContext.SaveChanges();
            item = MarkDownItemField(item);
            return View("ItemPage", new ItemAndCollection() { item = item, collection = collection, LikesCount = GetLikesCount(item.Likes) });
        }

        public IActionResult ResetLike(int ItemId)
        {
            Item item = _appDBContext.Items.Where(s => s.IDItem == ItemId).First();
            Collection collection = _appDBContext.Collections.Where(s => s.ID == item.IdCollection).FirstOrDefault();
            item.Likes = item.Likes.Replace(User.Identity.Name + ",","");
            item.LikesCount--;
            _appDBContext.Items.Update(item);
            collection.LikeCount--;
            _appDBContext.Collections.Update(collection);
            _appDBContext.SaveChanges();
            item = MarkDownItemField(item);
            return View("ItemPage", new ItemAndCollection() { item = item, collection = collection,LikesCount = GetLikesCount(item.Likes) });
        }

        public Item MarkDownItemField(Item item)
        {
            item.FirstField_String = markdown.Transform(item.FirstField_String);
            item.SecondField_String = markdown.Transform(item.SecondField_String);
            item.ThirdField_String = markdown.Transform(item.ThirdField_String);
            return item;
        }
        public IActionResult EditItem(int id)
        {
            var item = _appDBContext.Items.Where(s => s.IDItem == id).FirstOrDefault();
            var coll = _appDBContext.Collections.Where(s => s.ID == item.IdCollection).FirstOrDefault();
            EditItemViewModel mod = new EditItemViewModel()
            {
                CollectionsId = coll,
                FirstFiled_Int = item.FirstField_Int,
                FirstFiled_Data = item.FirstField_Data,
                FirstFiled_Bool = item.FirstField_Bool,
                FirstFiled_String = item.FirstField_String,
                SecondFiled_Bool = item.SecondField_Bool,
                SecondFiled_Data = item.SecondField_Data,
                SecondFiled_Int = item.SecondField_Int,
                SecondFiled_String = item.SecondField_String,
                ThirdFiled_Bool = item.ThirdField_Bool,
                ThirdFiled_Data = item.ThirdField_Data,
                ThirdFiled_Int = item.ThirdField_Int,
                ThirdFiled_String = item.ThirdField_String,
                ItemName = item.NameItem,
                Tags = item.Tags
            };
            mod.FirstList = Parse(coll.FirsList);
            mod.SecondList = Parse(coll.SecondList);
            mod.ThirdList = Parse(coll.ThirdList);
            return View(mod);
        }

       
            [HttpPost]
        public IActionResult EditItem(EditItemViewModel model)
        {
            var item = _appDBContext.Items.Where(s => s.IDItem == model.Id).FirstOrDefault();
            var coll = _appDBContext.Collections.Where(s => s.ID == item.IdCollection).FirstOrDefault();
            if (ModelState.IsValid)
            {            
                item.FirstField_Int = model.FirstFiled_Int;
                item.FirstField_Data = model.FirstFiled_Data==DateTime.MinValue?DateTime.Now:model.FirstFiled_Data;
                item.FirstField_Bool = model.FirstFiled_Bool;
                item.FirstField_String = model.FirstFiled_String;
                item.SecondField_Bool = model.SecondFiled_Bool;
                item.SecondField_Data = model.SecondFiled_Data== DateTime.MinValue?DateTime.Now:model.SecondFiled_Data;
                item.SecondField_Int = model.SecondFiled_Int;
                item.SecondField_String = model.SecondFiled_String;
                item.ThirdField_Bool = model.ThirdFiled_Bool;
                item.ThirdField_Data =model.ThirdFiled_Data==DateTime.MinValue?DateTime.Now:model.ThirdFiled_Data;
                item.ThirdField_Int = model.ThirdFiled_Int;
                item.ThirdField_String = model.ThirdFiled_String;
                item.NameItem = model.ItemName;
                item.Tags = model.Tags;
                _appDBContext.Items.Update(item);
                _appDBContext.SaveChanges();
                return RedirectToAction("CollectionPage", new { CollectionId = item.IdCollection });
            }
            model.FirstList = Parse(coll.FirsList);
            model.SecondList = Parse(coll.SecondList);
            model.ThirdList = Parse(coll.ThirdList);
            return View(model);
        }

        public IActionResult EditCollection(int id)
        {
            var collection = _appDBContext.Collections.Where(s => s.ID == id).First();
            var model = new EdditCollectionViewModel() { CollectionsName = collection.CollectionName, CollectionsTopic = collection.Topic, Description = collection.Description,Id=id,FirstFieldName=collection.FirstFieldName,SecondFieldName=collection.SecondFieldName,ThirdFieldName=collection.ThirdFieldName,FirstField=collection.FirstField,SecondField=collection.SecondFiled,ThirdField=collection.ThirdFiled,FirstList=collection.FirsList,SecondList=collection.SecondList,ThirdList=collection.ThirdList };  
            return View(model);
        }

        [HttpPost]
        public IActionResult EditCollection(EdditCollectionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var col = _appDBContext.Collections.Where(s => s.ID == model.Id).First();
                col.CollectionName = model.CollectionsName;
                col.Description = model.Description;
                col.Topic = model.CollectionsTopic;
                col.FirstFieldName = model.FirstFieldName;
                col.SecondFieldName = model.SecondFieldName;
                col.ThirdFieldName = model.ThirdFieldName;
                _appDBContext.Collections.Update(col);
                _appDBContext.SaveChanges();
                return RedirectToAction("MyColl",new {Username=col.UserName });
            }
            return View(model);
        }

        public PartialViewResult All(int Id)
        {
            var model = _appDBContext.Comments.Where(s=>s.ItemId==Id).OrderBy(s=>s.Id).ToList();
            return PartialView("_CommentsPartial", model);
        }

        [HttpPost]
        public IActionResult AddComment(string Comm,int itemID)
        {
            var UserName = User.Identity.Name;
            AddCommentsCount(itemID);
            _appDBContext.Comments.Add(new CommentModel()
            {
                ItemId = itemID,
                Comment = Comm,
                UserId = UserName
            });
            _appDBContext.SaveChanges();
            return RedirectToAction("ItemPage", new { ItemId= itemID });
        }

        public void AddCommentsCount(int id)
        {
            var item = _appDBContext.Items.Where(s => s.IDItem == id).First();
            item.CommentsCount++;
            _appDBContext.Items.Update(item);
            _appDBContext.SaveChanges();
        }

        public IActionResult LikeComm(int id)
        {
            var Comment = _appDBContext.Comments.Where(s => s.Id == id).First();
            Comment.LikesUser += "/" + User.Identity.Name+ "/";
            _appDBContext.Update(Comment);
            _appDBContext.SaveChanges();
            return RedirectToAction("ItemPage", new { ItemId = Comment.ItemId });
        }
        public IActionResult ResetLikeComm(int id)
        {
            var Comment = _appDBContext.Comments.Where(s => s.Id == id).First();
            Comment.LikesUser= Comment.LikesUser.Replace( "/" + User.Identity.Name + "/","");
            _appDBContext.Update(Comment);
            _appDBContext.SaveChanges();
            return RedirectToAction("ItemPage", new { ItemId = Comment.ItemId });
        }
        [HttpPost]
        public List<string> AutoComplete()
        {
            var tags = (from Tags in this._appDBContext.Tags                           
                             select Tags.Tag).ToList();
            return tags;
        }

        public IActionResult ExportToCsV(int id)
        {
            StringBuilder sb = new StringBuilder();
            
            List<string> CollumName= GetCollumName();
            sb.AppendLine(string.Join(",", CollumName));

            foreach(var item in _appDBContext.Items.Where(s=>s.IdCollection==id).ToList())
            {
                sb.AppendLine($"{item.IDItem},{item.IdCollection},{item.NameItem},{item.FirstField_Int}," +
                    $"{(String.Empty + item.FirstField_String).Replace("\r\n","")},{item.FirstField_Data},{item.FirstField_Bool},{item.SecondField_Int}," +
                    $"{(String.Empty+ item.SecondField_String).Replace("\r\n", "")},{item.SecondField_Data},{item.SecondField_Bool},{item.ThirdField_Int}," +
                    $"{(String.Empty + item.ThirdField_String).Replace("\r\n", "")},{item.ThirdField_Data},{item.ThirdField_Bool},{(String.Empty+item.Likes).Replace(",","/")},{item.Tags},{item.LikesCount},{item.CommentsCount}");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Items.csv");
        }

        public List<string> GetCollumName()
        {
            
            List<string> collumName=new List<string>() { "IDItem", "IdCollection", "NameItem",
                "FirstField_Int", "FirstField_String", "FirstField_Data", "FirstField_Bool",
                "SecondField_Int", "SecondField_String","SecondField_Data", "SecondField_Bool",
                "ThirdField_Int", "ThirdField_String", "ThirdField_Data", "ThirdField_Bool",
                "Likes", "Tags", "LikesCount", "CommentsCount" };
           
            return collumName;
        }
    }
}
