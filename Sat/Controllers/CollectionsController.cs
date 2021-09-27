using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Task4Core.Models;
using Task4Core.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static System.String;

namespace Task4Core.Controllers
{
    
    public class CollectionsController : Controller
    {
        readonly AppDbContext _appDbContext;
        private readonly MarkdownSharp.Markdown _markdown = new();
        IEnumerable<Collection> _collections;
       
        public CollectionsController( AppDbContext appDbContext)

        {
            _appDbContext = appDbContext;
        }

        public IActionResult Index(string topic,string sortOrder,string searchString)
        {

            FetchData(topic);
            ViewBag.NameSortParm = sortOrder=="Name" ? "name_desc" : "Name";           
            ViewBag.LikeSortParm = sortOrder == "Like" ? "like_desc" : "Like";
            ViewBag.ItemSortParm = sortOrder == "Item" ? "item_desc" : "Item";
            ViewData["CurrentFilter"] = searchString;
            SortCollections(sortOrder);
            _collections = searchString==null?_collections: SearchCollection(searchString);

            return View(_collections.ToList());

        }

        
        private void SortCollections(string sortOrder)
        {
            switch (sortOrder)
            {
                case "name_desc":
                    _collections = _collections.OrderByDescending(s => s.CollectionName);
                    break;
                case "Name":
                    _collections = _collections.OrderBy(s => s.CollectionName);
                    break;
                case "Like":
                    _collections = _collections.OrderBy(s => s.LikeCount);
                    break;
                case "like_desc":
                    _collections = _collections.OrderByDescending(s => s.LikeCount);
                    break;
                case "item_desc":
                    _collections = _collections.OrderByDescending(s => s.ItemCount);
                    break;
                case "Item":
                    _collections = _collections.OrderBy(s => s.ItemCount);
                    break;
                default:
                    _collections = _collections.OrderBy(s => s.ID);
                    break;
            }
        }

        private void FetchData(string collection)
        {
            _collections = collection != null ? _appDbContext.Collections.Where(i => i.Topic == collection) : _appDbContext.Collections;
        }
        public IActionResult AddItem(int collectionId)
        {
         
            return View(new AddItemViewModel() {Id=_appDbContext.Collections.First(s => s.ID==collectionId) });
        }
        [HttpPost]
        public IActionResult AddItem(AddItemViewModel model,int collectionId,IEnumerable<string> tags)
        {                           
            if (ModelState.IsValid)
            {
                var enumerable = tags as string[] ?? tags.ToArray();
                _appDbContext.Items.Add(new Item
                {
                    NameItem = model.ItemName,
                    IdCollection = collectionId,
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
                    Tags = TagsToString(enumerable)
                });
                AddTags(enumerable);
               
                return RedirectToAction("MyColl",new { Username= AddItemCount(collectionId).UserName});
            }         
            return View(SetCollectionsInModel(collectionId));
        }

        private Collection AddItemCount(int id)
        {
            var collectionInfo = _appDbContext.Collections.First(i => i.ID == id);
            collectionInfo.ItemCount++;
            _appDbContext.Collections.Update(collectionInfo);
            _appDbContext.SaveChanges();
            return collectionInfo;
        }

        private AddItemViewModel SetCollectionsInModel (int collectionId)
        {
            var collectionInfo = _appDbContext.Collections.First(i => i.ID == collectionId);
            return new AddItemViewModel() { Id = collectionInfo, FirstList = Parse(collectionInfo.FirsList), SecondList = Parse(collectionInfo.SecondList), ThirdList = Parse(collectionInfo.ThirdList) }; 
        }
        private string TagsToString(IEnumerable<string> tags)
        {
            string tagsString = Empty;
            foreach (var s in tags)
            {
                tagsString += '#' + s;
            }
            return tagsString;
        } 
        public List<string> Parse(string str)
        {
            return str?.Split(",").ToList();
        }
        private void AddTags(IEnumerable<string> tags)
        {
            foreach(var tag in tags)
            {
                if (!_appDbContext.Tags.Any(s => s.Tag == tag))
                {
                    _appDbContext.Tags.Add(new Tags() { Tag=tag});                    
                }
            }
            _appDbContext.SaveChanges();
        }
        public IActionResult AddCollection(string username)
        {
            if (User.Identity != null)
            {
                var userName = username ?? User.Identity.Name;
                return View(new AddCollectionsViewModel() {UserName= userName });
            }

            return null;
        }
        [HttpPost]
        public IActionResult AddCollection(AddCollectionsViewModel model,IEnumerable<string> list,IEnumerable<string> fieldName, IEnumerable<string> fieldListName, string username)
        {
            var userName = username ?? User.Identity?.Name;
            model.UserName = userName;
            if (ModelState.IsValid){
                var type = list.ToList();
                var name = fieldName.ToList();
                var listField = fieldListName.ToList();
                _appDbContext.Collections.Add(new Collection { CollectionName = model.CollectionsName, 
                    Topic = model.CollectionsTopic,
                    UserName = userName,
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
                _appDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }
        public IActionResult MyColl(string username)
        {
            var userName = username ?? User.Identity.Name;
            return View(new UserAndCollections() { collections = (_appDbContext.Collections.Where(i => i.UserName == userName).ToList()), UserName = userName });
        }
        
        public IActionResult DeleteCollections(int id)
        {
           var coll = _appDbContext.Collections.First(s => s.ID == id);
           var items = _appDbContext.Items.Where(s => s.IdCollection == coll.ID).ToList();
           foreach(var it in items)
           {
               DeleteItem(it.IDItem);
           }
           _appDbContext.Collections.Remove(coll);
           _appDbContext.SaveChanges();
           return RedirectToAction("MyColl", new {Username=coll.UserName });
        }
        
        public IActionResult DeleteItem(int id)
        {
            var item = _appDbContext.Items.First(s => s.IDItem == id);
            DeleteComments(id);
            _appDbContext.Items.Remove(item);
            _appDbContext.SaveChanges();
            return RedirectToAction("CollectionPage", new { CollectionId = RemoveComAndLike(GetLikesCount(item.Likes),item.IdCollection).ID });
        }
        private Collection RemoveComAndLike(int likeCount,int idCollection)
        {
            var col = _appDbContext.Collections.First(s => s.ID == idCollection);
            col.LikeCount -= likeCount;
            col.ItemCount--;
            _appDbContext.Collections.Update(col);
            _appDbContext.SaveChanges();
            return col;
        }
        public void DeleteComments(int id)
        {
            var comments = _appDbContext.Comments.Where(s => s.ItemId == id).ToList();
            foreach(var com in comments) {
                _appDbContext.Comments.Remove(com);
            }
        }
        public IActionResult CollectionPage(int collectionId, string sortOrder, string searchString)
        {
            var items = _appDbContext.Items.Where(i => i.IdCollection == collectionId);           
            var collection = _appDbContext.Collections.Where(I => I.ID == collectionId).ToList().First();
            var markdown = new MarkdownSharp.Markdown();
            collection.Description = markdown.Transform(collection.Description);
            ViewBag.NameSortParm = sortOrder == "Name" ? "name_desc" : "Name";
            ViewBag.DateSortParm = sortOrder == "Like" ? "like_sedc" : "Like";
            ViewBag.CommentsSortParm = sortOrder == "Comments" ? "comments_desc" : "Comments";
            items = SearchItem(searchString, items);
            items = SortItems(sortOrder,items);
           return View(new CollectionsItems() { collections = collection, items = items.ToList() });
        }

        private IQueryable<Item> SortItems(string sortOrder, IQueryable<Item> items)
        {
            items = sortOrder switch
            {
                "name_desc" => items.OrderByDescending(s => s.NameItem),
                "Like" => items.OrderBy(s => s.LikesCount),
                "Name" => items.OrderBy(s => s.NameItem),
                "like_sedc" => items.OrderByDescending(s => s.LikesCount),
                "Comments" => items.OrderBy(s => s.CommentsCount),
                "comments_desc" => items.OrderByDescending(s => s.CommentsCount),
                _ => items.OrderBy(s => s.IDItem)
            };
            return items;
        }
        private IQueryable<Item>  SearchItem(string searchString, IQueryable<Item> items)
        {
            if (!IsNullOrEmpty(searchString))
            {
                items = items.Where(s => s.NameItem.Contains(searchString));
            }
            return items;
        }
        public IActionResult Items(string searchString)
        {
           
            if (!IsNullOrEmpty(searchString))
            {          
                List<Item> items = new();
                AddItemsToListFromCollection(ref items, SearchCollection(searchString).ToList());              
                AddItemsToListFromComments(ref items, new HashSet<int>(SearchComments(searchString)));
                items.AddRange(SearchItem(searchString).ToList());
                var uniqueItems = new HashSet<Item>(items);
                return View(uniqueItems.ToList());
            } 
            return RedirectToAction("Index");
        }

        private void AddItemsToListFromCollection( ref List<Item> items,List<Collection> listcoll)
        {
            foreach (var col in listcoll)
            {
                var list = _appDbContext.Items.Where(s => s.IdCollection == col.ID);
                items.AddRange(list);
            }
           
        }
        private void AddItemsToListFromComments( ref List<Item> items, HashSet<int> listIdComm )
        {
            foreach (var id in listIdComm)
            {
                var list = _appDbContext.Items.Where(s => s.IDItem == id);
                items.AddRange(list);
            }
           
        }


        private IQueryable<Item> SearchItem(string searchString)
        {
            var results = from p in _appDbContext.Items
                          where EF.Functions.Contains(p.NameItem, searchString) || EF.Functions.Contains(p.Tags, searchString) || EF.Functions.Contains(p.FirstField_String, searchString) || EF.Functions.Contains(p.SecondField_String, searchString) || EF.Functions.Contains(p.ThirdField_String, searchString)
                          select p;
            return results;
        }
        private IQueryable<Collection> SearchCollection(string searchString)
        {
            var results = from p in _appDbContext.Collections
                          where EF.Functions.Contains(p.Description, searchString)|| EF.Functions.Contains(p.CollectionName, searchString)
                          select p;
            return results;
        }
        private IQueryable<int> SearchComments(string searchString)
        {
            var comments = from p in _appDbContext.Comments
                           where EF.Functions.Contains(p.Comment, searchString)
                           select p.ItemId;
            return comments;
        }
        public IActionResult ItemPage(int itemId)
        {
            var res = _appDbContext.Items.First(s => s.IDItem == itemId);
            var markdown = new MarkdownSharp.Markdown();
            res.FirstField_String = markdown.Transform(res.FirstField_String);
            res.SecondField_String = markdown.Transform(res.SecondField_String);
            res.ThirdField_String = markdown.Transform(res.ThirdField_String);
            var col = _appDbContext.Collections.First(s => s.ID == res.IdCollection);
            return View(new ItemAndCollection() { collection = col, item = res,LikesCount=GetLikesCount(res.Likes) });
        }

        public int GetLikesCount(string likeString)
        {
            return likeString!=null?(likeString.Length - likeString.Replace(",", "").Length):0;
        }
        public IActionResult AddLike(int itemId)
        {
            var item = _appDbContext.Items.First(s => s.IDItem == itemId);
            var collection = _appDbContext.Collections.FirstOrDefault(s => s.ID == item.IdCollection);
            item.Likes +=User.Identity.Name+",";
            item.LikesCount++;
            _appDbContext.Items.Update(item);
            collection.LikeCount++;
            _appDbContext.Collections.Update(collection);
            _appDbContext.SaveChanges();
            item = MarkDownItemField(item);
            return View("ItemPage", new ItemAndCollection() { item = item, collection = collection, LikesCount = GetLikesCount(item.Likes) });
        }

        public IActionResult ResetLike(int itemId)
        {
            Item item = _appDbContext.Items.First(s => s.IDItem == itemId);
            Collection collection = _appDbContext.Collections.FirstOrDefault(s => s.ID == item.IdCollection);
            item.Likes = item.Likes.Replace(User.Identity?.Name + ",","");
            item.LikesCount--;
            _appDbContext.Items.Update(item);
            collection.LikeCount--;
            _appDbContext.Collections.Update(collection);
            _appDbContext.SaveChanges();
            item = MarkDownItemField(item);
            return View("ItemPage", new ItemAndCollection() { item = item, collection = collection,LikesCount = GetLikesCount(item.Likes) });
        }

        public Item MarkDownItemField(Item item)
        {
            item.FirstField_String = _markdown.Transform(item.FirstField_String);
            item.SecondField_String = _markdown.Transform(item.SecondField_String);
            item.ThirdField_String = _markdown.Transform(item.ThirdField_String);
            return item;
        }
        public IActionResult EditItem(int id)
        {
            var item = _appDbContext.Items.FirstOrDefault(s => s.IDItem == id);
            var coll = _appDbContext.Collections.FirstOrDefault(s => s.ID == item.IdCollection);
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
            var item = _appDbContext.Items.FirstOrDefault(s => s.IDItem == model.Id);
            var coll = _appDbContext.Collections.FirstOrDefault(s => s.ID == item.IdCollection);
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
                _appDbContext.Items.Update(item);
                _appDbContext.SaveChanges();
                return RedirectToAction("CollectionPage", new { CollectionId = item.IdCollection });
            }
            model.FirstList = Parse(coll.FirsList);
            model.SecondList = Parse(coll.SecondList);
            model.ThirdList = Parse(coll.ThirdList);
            return View(model);
        }

        public IActionResult EditCollection(int id)
        {
            var collection = _appDbContext.Collections.First(s => s.ID == id);
            var model = new EdditCollectionViewModel() { CollectionsName = collection.CollectionName, CollectionsTopic = collection.Topic, Description = collection.Description,Id=id,FirstFieldName=collection.FirstFieldName,SecondFieldName=collection.SecondFieldName,ThirdFieldName=collection.ThirdFieldName,FirstField=collection.FirstField,SecondField=collection.SecondFiled,ThirdField=collection.ThirdFiled,FirstList=collection.FirsList,SecondList=collection.SecondList,ThirdList=collection.ThirdList };  
            return View(model);
        }

        [HttpPost]
        public IActionResult EditCollection(EdditCollectionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var col = _appDbContext.Collections.Where(s => s.ID == model.Id).First();
                col.CollectionName = model.CollectionsName;
                col.Description = model.Description;
                col.Topic = model.CollectionsTopic;
                col.FirstFieldName = model.FirstFieldName;
                col.SecondFieldName = model.SecondFieldName;
                col.ThirdFieldName = model.ThirdFieldName;
                _appDbContext.Collections.Update(col);
                _appDbContext.SaveChanges();
                return RedirectToAction("MyColl",new {Username=col.UserName });
            }
            return View(model);
        }

        public PartialViewResult All(int id)
        {
            var model = _appDbContext.Comments.Where(s=>s.ItemId==id).OrderBy(s=>s.Id).ToList();
            return PartialView("_CommentsPartial", model);
        }

        [HttpPost]
        public IActionResult AddComment(string comm,int itemId)
        {
            var userName = User.Identity.Name;
            AddCommentsCount(itemId);
            _appDbContext.Comments.Add(new CommentModel()
            {
                ItemId = itemId,
                Comment = comm,
                UserId = userName
            });
            _appDbContext.SaveChanges();
            return RedirectToAction("ItemPage", new { ItemId= itemId });
        }

        public void AddCommentsCount(int id)
        {
            var item = _appDbContext.Items.First(s => s.IDItem == id);
            item.CommentsCount++;
            _appDbContext.Items.Update(item);
            _appDbContext.SaveChanges();
        }

        public IActionResult LikeComm(int id)
        {
            var comment = _appDbContext.Comments.First(s => s.Id == id);
            comment.LikesUser += "/" + User.Identity.Name+ "/";
            _appDbContext.Update(comment);
            _appDbContext.SaveChanges();
            return RedirectToAction("ItemPage", new {comment.ItemId });
        }
        public IActionResult ResetLikeComm(int id)
        {
            var comment = _appDbContext.Comments.First(s => s.Id == id);
            comment.LikesUser= comment.LikesUser.Replace( "/" + User.Identity.Name + "/","");
            _appDbContext.Update(comment);
            _appDbContext.SaveChanges();
            return RedirectToAction("ItemPage", new { ItemId = comment.ItemId });
        }
        [HttpPost]
        public List<string> AutoComplete()
        {
            var tagsList = (from tags in this._appDbContext.Tags                           
                             select tags.Tag).ToList();
            return tagsList;
        }

        public IActionResult ExportToCsV(int id)
        {
            StringBuilder sb = new StringBuilder();
            
            List<string> collumName= GetCollumName();
            sb.AppendLine(Join(",", collumName));

            foreach(var item in _appDbContext.Items.Where(s=>s.IdCollection==id).ToList())
            {
                sb.AppendLine($"{item.IDItem},{item.IdCollection},{item.NameItem},{item.FirstField_Int}," +
                    $"{(Empty + item.FirstField_String).Replace("\r\n","")},{item.FirstField_Data},{item.FirstField_Bool},{item.SecondField_Int}," +
                    $"{(Empty+ item.SecondField_String).Replace("\r\n", "")},{item.SecondField_Data},{item.SecondField_Bool},{item.ThirdField_Int}," +
                    $"{(Empty + item.ThirdField_String).Replace("\r\n", "")},{item.ThirdField_Data},{item.ThirdField_Bool},{(Empty+item.Likes).Replace(",","/")},{item.Tags},{item.LikesCount},{item.CommentsCount}");
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
