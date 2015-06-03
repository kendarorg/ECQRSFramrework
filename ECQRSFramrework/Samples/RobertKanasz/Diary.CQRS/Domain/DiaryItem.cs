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
using Diary.CQRS.Domain.Mementos;
using Diary.CQRS.Events;
using ECQRS.Commons.Domain;

namespace Diary.CQRS.Domain
{
    public class DiaryItem : AggregateRootSnapshottable
    {
        private Guid _id;
        public string Title { get; set; }

        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Description { get; set; }

        public DiaryItem()
        {
            
        }

        public DiaryItem(Guid id,string title, string description,  DateTime from, DateTime to)
        {
            ApplyChange(new ItemCreatedEvent(id, title,description, from, to));
        }

        public void ChangeTitle(string title)
        {
            ApplyChange(new ItemRenamedEvent(Id, title));
        }

        public void ChangeDescription(string description)
        {
            ApplyChange(new ItemDescriptionChangedEvent(Id, description));
        }

        public void ChangeFrom(DateTime from)
        {
            ApplyChange(new ItemFromChangedEvent(Id, from));
        }

        public void ChangeTo(DateTime to)
        {
            ApplyChange(new ItemToChangedEvent(Id, to));
        }

        public void Delete()
        {
            ApplyChange(new ItemDeletedEvent(Id));
        }

        protected void Apply(ItemDeletedEvent e)
        {
            
        }

        protected void Apply(ItemCreatedEvent e)
        {
            Title = e.Title;
            From = e.From;
            To = e.To;
            _id = e.AggregateId;
            Description = e.Description;
        }

        protected void Apply(ItemRenamedEvent e)
        {
            Title = e.Title;
        }

        protected void Apply(ItemFromChangedEvent e)
        {
            From = e.From;
        }

        protected void Apply(ItemToChangedEvent e)
        {
            To = e.To;
        }

        protected void Apply(ItemDescriptionChangedEvent e)
        {
            Description = e.Description;
        }
     
        public override Memento GetMemento()
        {
            return new DiaryItemMemento(Id,Title, Description,From,To,Version);
        }

        public override void SetMemento(Memento memento)
        {
            Title = ((DiaryItemMemento) memento).Title;
            To = ((DiaryItemMemento)memento).To;
            From = ((DiaryItemMemento)memento).From;
            Description = ((DiaryItemMemento)memento).Description;
            _id = memento.Id;
            base.SetMemento(memento);
        }

        public override Guid Id
        {
            get { return _id; }
        }
    }
}
