using Microsoft.AspNetCore.Mvc;
using Sat.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Task4Core.Models;
using Sat.Models;
using Task4Core.ViewModels;
using System.Data.Entity;
using System.Configuration;
using Microsoft.Extensions.Configuration;

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
            var temp = _appDBContext.Collections.ToList();
            FetchData(Topic);
            ViewBag.NameSortParm = sortOrder=="Name" ? "name_desc" : "Name";           
            ViewBag.LikeSortParm = sortOrder == "Like" ? "like_desc" : "Like";
            ViewBag.ItemSortParm = sortOrder == "Item" ? "item_desc" : "Item";
            ViewData["CurrentFilter"] = searchString;
            if (!String.IsNullOrEmpty(searchString))
            {
                collections = collections.Where(s => s.CollectionName.Contains(searchString));
            }
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
            return View(collections.ToList());
        }

        private void FetchData(string collection)
        {
            if (collection != null)
                collections = _appDBContext.Collections.Where(i => i.Topic == collection);
            else
                collections = _appDBContext.Collections;
        }
        
        public IActionResult AddItem(Item item)
        {

            return View();
        }
        [HttpPost]
        public IActionResult AddItem(AddItemViewModel model,int CollectionId,IEnumerable<string> Tags)
        {
            var CollectionInfo = _appDBContext.Collections.Where(i => i.ID == CollectionId).First();
            DateTime time1=DateTime.Now, time2=DateTime.Now, time3=DateTime.Now;
            string tagsString=String.Empty;
            if (ModelState.IsValid)
            {
                foreach(var s in Tags)
                {
                    tagsString += '#'+s;
                }

                if (model.FirstFiled_Data != DateTime.MinValue)
                {
                    time1 = model.FirstFiled_Data;
                }
                if (model.SecondFiled_Data != DateTime.MinValue) {
                    time2 = model.SecondFiled_Data;
                };
                if (model.ThirdFiled_Data != DateTime.MinValue) {
                    time3 = model.ThirdFiled_Data;
                }
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
                    FirstField_Data = time1,
                    SecondField_Data = time2,
                    ThirdField_Data = time3,
                    Tags = tagsString
                }) ;
                CollectionInfo.ItemCount++;
                _appDBContext.Collections.Update(CollectionInfo);
                _appDBContext.SaveChanges();
                return RedirectToAction("MyColl",new { Username=CollectionInfo.UserName});
            }
            
            return View(new AddItemViewModel() {Id= CollectionInfo,FirstFiled_Data=DateTime.Now,SecondFiled_Data=DateTime.Now,ThirdFiled_Data=DateTime.Now});
        }

        public IActionResult AddCollection()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddCollection(AddCollectionsViewModel model,IEnumerable<string> list,IEnumerable<string> FieldName,string Username)
        {
            var UserName = Username ?? User.Identity.Name;
            model.UserName = UserName;
            if (ModelState.IsValid){
                List<string> type = list.ToList();
                List<string> name = FieldName.ToList();
                _appDBContext.Collections.Add(new Collection { CollectionName = model.CollectionsName, Topic = model.CollectionsTopic,
                    UserName = UserName,
                    Description=model.Description,
                    FirstField=type.Count>=1?type[0]:null,
                    SecondFiled= type.Count >= 2 ? type[1] : null,
                    ThirdFiled= type.Count >= 3 ? type[2] : null,
                    FirstFieldName=name.Count>=1?name[0]:null,
                    SecondFieldName = name.Count >= 2 ? name[1] : null,
                    ThirdFieldName = name.Count >= 3 ? name[2] : null,
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
            var col=_appDBContext.Collections.Where(s => s.ID == item.IdCollection).First();
            col.LikeCount -= GetLikesCount(item.Likes);
            col.ItemCount--;
            _appDBContext.Collections.Update(col);
            _appDBContext.SaveChanges();
            return RedirectToAction("CollectionPage", new { CollectionId = col.ID });
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
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            if (!String.IsNullOrEmpty(searchString))
            {
                Items = Items.Where(s => s.NameItem.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    Items = Items.OrderByDescending(s => s.NameItem);
                    break;
                case "Date":
                    Items = Items.OrderBy(s => s.IDItem);
                    break;
                case "Name":
                    Items = Items.OrderBy(s => s.NameItem);
                    break;
                case "date_desc":
                    Items = Items.OrderByDescending(s => s.IDItem);
                    break;
                default:
                    Items = Items.OrderBy(s => s.IDItem);
                    break;
            }
            
            return View(new CollectionsItems() { collections = collection, items = Items });
        }

        public IActionResult Items(string searchString)
        {
           
            if (!String.IsNullOrEmpty(searchString))
            {
                var items = _appDBContext.Items.Where(s => s.NameItem.Contains(searchString)||
                                                        s.Tags.Contains(searchString));
                return View(items.ToList());
            }
            return RedirectToAction("Index");
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
            _appDBContext.Items.Update(item);
            collection.LikeCount++;
            _appDBContext.Collections.Update(collection);
            _appDBContext.SaveChanges();
            item = Mar(item);
            return View("ItemPage", new ItemAndCollection() { item = item, collection = collection, LikesCount = GetLikesCount(item.Likes) });
        }
        public IActionResult ResetLike(int ItemId)
        {
            Item item = _appDBContext.Items.Where(s => s.IDItem == ItemId).First();
            Collection collection = _appDBContext.Collections.Where(s => s.ID == item.IdCollection).FirstOrDefault();
            item.Likes = item.Likes.Replace(User.Identity.Name + ",","");
            _appDBContext.Items.Update(item);
            collection.LikeCount--;
            _appDBContext.Collections.Update(collection);
            _appDBContext.SaveChanges();
            item = Mar(item);
            return View("ItemPage", new ItemAndCollection() { item = item, collection = collection,LikesCount = GetLikesCount(item.Likes) });
        }

        public Item Mar(Item item)
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
            return View(mod);
        }
            [HttpPost]
        public IActionResult EditItem(EditItemViewModel model)
        {
            var item = _appDBContext.Items.Where(s => s.IDItem == model.Id).FirstOrDefault();
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
            return View(model);
        }

        public IActionResult EditCollection(int id)
        {
            var collection = _appDBContext.Collections.Where(s => s.ID == id).First();
            var model = new EdditCollectionViewModel() { CollectionsName = collection.CollectionName, CollectionsTopic = collection.Topic, Description = collection.Description,Id=id };
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
                _appDBContext.Collections.Update(col);
                _appDBContext.SaveChanges();
                return RedirectToAction("MyColl",new {Username=col.UserName });
            }
            return View(model);
        }

        public PartialViewResult All(int Id)
        {
            var model = _appDBContext.Comments.Where(s=>s.ItemId==Id).ToList();
            return PartialView("_CommentsPartial", model);
        }

        [HttpPost]
        public IActionResult AddComment(string Comm,int itemID)
        {
            var UserName = User.Identity.Name;
            _appDBContext.Comments.Add(new CommentModel()
            {
                ItemId = itemID,
                Comment = Comm,
                UserId = UserName
            });
            _appDBContext.SaveChanges();
            return RedirectToAction("ItemPage", new { ItemId= itemID });
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
    }
}
