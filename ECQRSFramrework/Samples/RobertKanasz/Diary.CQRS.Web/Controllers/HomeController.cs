// ===========================================================
// Copyright (c) 2014-2015, Enrico Da Ros/kendar.org
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// ===========================================================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Diary.CQRS.Commands;
using Diary.CQRS.Reporting;
using ECQRS.Commons.Commands;
using ECQRS.Commons.Exceptions;
using ECQRS.Commons.Repositories;

namespace Diary.CQRS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICommandSender _bus;
        private IRepository<DiaryItemDto> _reportDatabase;

        public HomeController(ICommandSender bus,IRepository<DiaryItemDto> reportDatabase)
        {
            _bus = bus;
            _reportDatabase = reportDatabase;
        }

        public ActionResult Index()
        {
            ViewBag.Model = _reportDatabase.GetAll();
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }

        public ActionResult Delete(Guid id)
        {
            var item = _reportDatabase.Get(id);
            _bus.Send(new DeleteItemCommand(item.Id,item.Version));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Add(DiaryItemDto item)
        {
            _bus.Send(new CreateItemCommand(Guid.NewGuid(),item.Title,item.Description,-1,item.From,item.To));

            return RedirectToAction("Index");
        }

        public ActionResult Edit(Guid id)
        {
            var item = _reportDatabase.Get(id);
            var model = new DiaryItemDto()
                {
                    Description = item.Description,
                    From = item.From,
                    Id = item.Id,
                    Title = item.Title,
                    To = item.To,
                    Version = item.Version
                };
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(DiaryItemDto item)
        {
            try
            {
                _bus.Send(new ChangeItemCommand(item.Id, item.Title, item.Description, item.From, item.To, item.Version));
            }
            catch (ConcurrencyException err)
            {

                ViewBag.error = err.Message;
                ModelState.AddModelError("", err.Message);
                return View();

            }
            
            return RedirectToAction("Index");
        }
    }
}
