﻿using System;
using System.IO;
using System.Linq;
using PivotalTracker.FluentAPI.Domain;
using PivotalTracker.FluentAPI.Repository;

namespace PivotalTracker.FluentAPI.Service
{
    /// <summary>
    /// Facade to manage a story
    /// </summary>
    /// <typeparam name="TParent">Parent facade Type. This Facade can have multiple Parent, so it stays generic</typeparam>
    public class StoryFacade<TParent> : FacadeItem<StoryFacade<TParent>, TParent, Story>
        where TParent:IFacade
    {
        private readonly PivotalStoryRepository _storyRepository;
        private readonly PivotalAttachmentRepository _attachRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Parent Facade</param>
        /// <param name="item">Story to manage</param>
        public StoryFacade(TParent parent, Story item)
            : base(parent, item)
        {
            _storyRepository = new Repository.PivotalStoryRepository(this.RootFacade.Token);
            _attachRepository = new PivotalAttachmentRepository(this.RootFacade.Token);
        }

        /// <summary>
        /// Call the action updater then send the modification to Pivotal
        /// </summary>
        /// <param name="updater">Action that accepts a the story to modified (this.Item)</param>
        /// <returns>This</returns>
        public StoryFacade<TParent> Update(Action<Story> updater)
        {
            updater(Item);

            // Check if we can estimate bugs and chores
            var isBugAndChoresEstimables = false;
            var parent = ParentFacade as StoriesProjectFacade;
            if (parent != null)
                isBugAndChoresEstimables = parent.ParentFacade.Item.IsBugAndChoresEstimables;

            Item = _storyRepository.UpdateStory(Item, isBugAndChoresEstimables);

            return this;
        }

        /// <summary>
        /// Add a note to the managed story
        /// </summary>
        /// <param name="text">text of the note</param>
        /// <returns>This</returns>
        public StoryFacade<TParent> AddNote(string text)
        {
            var note = _storyRepository.AddNote(this.Item.ProjectId, this.Item.Id, text);
            this.Item.Notes.Add(note);
            return this;
        }

        /// <summary>
        /// Add a task to the managed story
        /// </summary>
        /// <param name="description">text of the task</param>
        /// <returns>This</returns>
        public StoryFacade<TParent> AddTask(string description)
        {
            var task = _storyRepository.AddTask(Item.ProjectId, Item.Id, description);
            Item.Tasks.Add(task);
            return this;
        }

        /// <summary>
        /// Add a task to the managed story
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="isCompleted">true if the task is completed, false otherwise.</param>
        /// <param name="description">text of the task</param>
        /// <returns>This</returns>
        public StoryFacade<TParent> UpdateTask(int id, bool isCompleted, string description)
        {
            _storyRepository.UpdateTask(Item.ProjectId, Item.Id, id, isCompleted, description);

            // Update task
            var oldTask = Item.Tasks.Single(t => t.Id == id);
            oldTask.Description = description;
            oldTask.IsCompleted = isCompleted;

            return this;
        }

        /// <summary>
        /// Remove a task from the managed story
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>This</returns>
        public StoryFacade<TParent> RemoveTask(int id)
        {
            var oldTask = Item.Tasks.Single(t => t.Id == id);
            var task = _storyRepository.RemoveTask(Item.ProjectId, Item.Id, id);

            Item.Tasks.Remove(oldTask);

            return this;
        }

        /// <summary>
        /// Delete the managed StoryFacade
        /// </summary>
        /// <returns>Parent Facade</returns>
        public TParent Delete()
        {
            _storyRepository.DeleteStory(this.Item.ProjectId, this.Item.Id);
            return Done();
        }

        /// <summary>
        /// Upload an attachment to the managed story
        /// </summary>
        /// <param name="attachment">data to upload</param>
        /// <param name="fileName">attachment filename in Pivotal</param>
        /// <param name="contentType">data content-type </param>
        /// <returns>This</returns>
        public StoryFacade<TParent> UploadAttachment(byte[] attachment, string fileName="upload", string contentType="application/octet-stream")
        {
            using (var stream = new MemoryStream(attachment))
            {
                return UploadAttachment((s, output) => stream.WriteTo(output), fileName, contentType);
            }
        }

        //TODO: Deviner le mime-type
        /// <summary>
        /// Upload an attachment to the managed story. The developper write into the stream
        /// </summary>
        /// <param name="action">Action that accepts the story and the upload stream</param>
        /// <param name="fileName">attachment filename in Pivotal</param>
        /// <param name="contentType">data content-type </param>
        /// <returns>This</returns>
        public StoryFacade<TParent> UploadAttachment(Action<Story, Stream> action, string fileName = "upload", string contentType = "application/octet-stream")
        {
            using (var stream = new MemoryStream())
            {
                action(this.Item, stream);
                _attachRepository.UploadAttachment(this.Item.ProjectId, this.Item.Id, stream.ToArray(), fileName, contentType);

                return this;
            }
        }

        /// <summary>
        /// Download a specific attachment
        /// </summary>
        /// <param name="a">attachment to download</param>
        /// <returns>attachment data</returns>
        public byte[] DownloadAttachment(Attachment a)
        {
            return _attachRepository.DownloadAttachment(a);
        }

        public StoryFacade<TParent> Move(int targetStoryId, bool before = true)
        {
            _storyRepository.MoveStory(this.Item.ProjectId, this.Item.Id, before ? PivotalStoryRepository.MovePositionEnum.Before : PivotalStoryRepository.MovePositionEnum.After, targetStoryId);

            return this;
        }

    }


}