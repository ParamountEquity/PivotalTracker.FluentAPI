using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using PivotalTracker.FluentAPI.Domain;

namespace PivotalTracker.FluentAPI.Repository
{
	/// <summary>
	/// This repository manages Pivotal stories
	/// </summary>
	/// <see cref="https://www.pivotaltracker.com/help/api?version=v3#getting_stories"/>
	/// <see cref="https://www.pivotaltracker.com/help/api?version=v3#manage_stories"/>
	public class PivotalStoryRepository : PivotalTrackerRepositoryBase
	{



		#region DTOs
		[XmlRoot("story")]
		public class StoryXmlResponse
		{
			public bool idSpecified { get { return id != 0; } }
			public int id { get; set; }
			public bool project_idSpecified { get { return project_id != 0; } }
			public int project_id { get; set; }
			public string story_type { get; set; }
			public string url { get; set; }
			[XmlIgnore]
			public bool estimateSpecified { get { return estimate >= -1; } }
			public int estimate { get; set; }
			public string current_state { get; set; }
			public string description { get; set; }
			public string name { get; set; }
			public string requested_by { get; set; }
			public string owned_by { get; set; }
			public DateTimeUTC created_at { get; set; }
			public DateTimeUTC updated_at { get; set; }
			public DateTimeUTC accepted_at { get; set; }
			public string labels { get; set; }

			[XmlArray("attachments")]
			[XmlArrayItem("attachment")]
			public Attachment[] attachments { get; set; }

			[XmlArray("notes")]
			[XmlArrayItem("note")]
			public StoryNoteXmlResponse[] notes { get; set; }

			[XmlArray("tasks")]
			[XmlArrayItem("task")]
			public StoryTaskXmlResponse[] tasks { get; set; }


		}

		[XmlRoot("story")]
		public class StoryXmlRequest : StoryXmlResponse
		{

		}

		[XmlRoot("stories")]
		public class StoriesXmlResponse
		{
			[XmlElement("story")]
			public StoryXmlResponse[] stories;
		}

		[XmlRoot("story")]
		public class StoryCreationXmlRequest
		{
			public string story_type { get; set; }
			public string name { get; set; }
			public string requested_by { get; set; }
			public string description { get; set; }
			public string labels { get; set; }
			public string current_state { get; set; }

			public string owned_by { get; set; }
			[XmlIgnore]
			public bool estimateSpecified { get { return estimate >= -1; } }
			public int estimate { get; set; }
		}

		[XmlRoot("note")]
		public class StoryNoteXmlRequest
		{
			public string text { get; set; }
		}

		[XmlRoot("note")]
		public class StoryNoteXmlResponse
		{
			public int id { get; set; }
			public string text { get; set; }
			public string author { get; set; }
			public DateTimeUTC noted_at { get; set; }
		}

		[XmlRoot("task")]
		public class StoryTaskXmlRequest
		{
			public string description { get; set; }
			public string complete { get; set; }
		}

		[XmlRoot("task")]
		public class StoryTaskXmlResponse
		{
			public int id { get; set; }
			public string description { get; set; }
			public int position { get; set; }
			public bool complete { get; set; }
			public DateTimeUTC created_at { get; set; }
		}

		#endregion


		public PivotalStoryRepository(Token token) : base(token)
		{
		}

		internal static Story CreateStory(StoryXmlResponse e)
		{

			var lStory = new Story()
							 {
								 AcceptedDate = e.accepted_at,
								 //Attachments =
								 CreatedDate = e.created_at,
								 UpdatedDate = e.updated_at,
								 CurrentState = (StoryStateEnum) Enum.Parse(typeof (StoryStateEnum), e.current_state, true),
								 Description = e.description,
								 Estimate =  e.estimate,
								 Id = e.id,
								 Name = e.name,
								 //Notes =
								 OwnedBy = e.owned_by,
								 ProjectId = e.project_id,
								 RequestedBy = e.requested_by,
								 //Tasks =
								 Type = (StoryTypeEnum)Enum.Parse(typeof(StoryTypeEnum), e.story_type, true),
								 Url = new Uri(e.url)
							 };

			if (e.attachments != null)
				e.attachments.ToList().ForEach(a => lStory.Attachments.Add(a));

			if (e.labels != null)
			{
				var labels = e.labels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				labels.ToList().ForEach(a => lStory.Labels.Add(a));
			}

			if (e.notes != null)
				e.notes.ToList().ForEach(a => lStory.Notes.Add(new Note()
				{
					Id = a.id,
					Author = a.author,
					Description = a.text,
					NoteDate = a.noted_at == null ? null : a.noted_at.DateTime
				}));

			if (e.tasks != null)
				e.tasks.ToList().ForEach(a => lStory.Tasks.Add(new Task()
				{
					Id = a.id,
					Description = a.description,
					Position = a.position,
					IsCompleted = a.complete,
					CreatedDate = a.created_at == null ? null : a.created_at.DateTime
				}));

			return lStory;
		}

		internal static Task CreateTask(StoryTaskXmlResponse e)
		{
			return new Task
				{
					Id = e.id,
					Description = e.description,
					Position = e.position,
					IsCompleted = e.complete,
					CreatedDate = e.created_at == null ? null : e.created_at.DateTime
				};
		}

		public IEnumerable<Story> GetStories(string url, string method="GET")
		{
			var e = this.RequestPivotal<StoriesXmlResponse>(url, null, method);
			if (e.stories != null)
				return e.stories.Select(CreateStory).ToList();
			return new List<Story>();
		}


		public Story GetStory(int projectId, int storyId)
		{
			var path = string.Format("/projects/{0}/stories/{1}", projectId, storyId);
			var e = this.RequestPivotal<StoryXmlResponse>(path, null, "GET");

			return PivotalStoryRepository.CreateStory(e);
		}

		public IEnumerable<Story> GetStories(int projectId)
		{
			var path = string.Format("/projects/{0}/stories", projectId);

			return GetStories(path);

		}

		//TODO: can GetSomeStories & GetLimitedStories be an unique method ?
		public IEnumerable<Story> GetSomeStories(int projectId, string filter)
		{
			var path = string.Format("/projects/{0}/stories?filter={1}", projectId, Uri.EscapeDataString(filter));
			return GetStories(path);

		}

		public IEnumerable<Story> GetLimitedStories(int projectId, int offset, int limit)
		{
			var path = string.Format("/projects/{0}/stories?limit={1}&offset={2}", projectId, limit, offset);
			return GetStories(path);

		}

		public Story AddStory(int projectId, StoryCreationXmlRequest storyCreationRequest)
		{
			var path = string.Format("/projects/{0}/stories", projectId);
			var e = this.RequestPivotal<StoryXmlResponse>(path, storyCreationRequest, "POST");
			return CreateStory(e);

		}

		public Story UpdateStory(Story story, bool isBugAndChoresEstimables)
		{
			var path = string.Format("/projects/{0}/stories/{1}", story.ProjectId, story.Id);
			var s = new StoryXmlRequest
						{
							current_state = story.CurrentState.ToString().ToLowerInvariant(),
							description = story.Description,
							labels = story.Labels.Count == 0 ? "" : story.Labels.Aggregate((a, b) => a+b),
							name = story.Name,
							owned_by = story.OwnedBy,
							story_type = story.Type.ToString().ToLowerInvariant(),
							requested_by = story.RequestedBy,
						};

			// Check if we should set an estimate
			if (story.Estimate >= -1)
			{
				switch (story.Type)
				{
					case StoryTypeEnum.Chore:
					case StoryTypeEnum.Bug:
						if (isBugAndChoresEstimables)
							s.estimate = story.Estimate;
						else
							s.estimate = -1;
						break;

					default:
						s.estimate = story.Estimate;
						break;
				}
			}


			var e = this.RequestPivotal<StoryXmlResponse>(path, s, "PUT");
			return CreateStory(e);

		}

		public Note AddNote(int projectId, int storyId, string text)
		{
			var path = string.Format("/projects/{0}/stories/{1}/notes", projectId, storyId);
			var noteReq = new Repository.PivotalStoryRepository.StoryNoteXmlRequest { text = text };

			var noteResp = this.RequestPivotal<StoryNoteXmlResponse>(path, noteReq, "POST");
			var note = new Note
						   {
							   Author = noteResp.author,
							   Description = noteResp.text,
							   NoteDate = noteResp.noted_at == null ? (DateTime?)null : noteResp.noted_at.DateTime.Value,
							   Id = noteResp.id,
							   StoryId = storyId
						   };
			return note;

		}

		public Task AddTask(int projectId, int storyId, string text)
		{
			var path = string.Format("/projects/{0}/stories/{1}/tasks", projectId, storyId);
			var taskReq = new StoryTaskXmlRequest { description = text };

			var taskResp = this.RequestPivotal<StoryTaskXmlResponse>(path, taskReq, "POST");
			return CreateTask(taskResp);
		}

		public Task UpdateTask(int projectId, int storyId, int taskId, bool isCompleted, string text)
		{
			var path = string.Format("/projects/{0}/stories/{1}/tasks/{2}", projectId, storyId, taskId);
			var taskReq = new StoryTaskXmlRequest
				{
					description = text,
					complete = isCompleted ? "true" : "false"
				};

			var taskResp = this.RequestPivotal<StoryTaskXmlResponse>(path, taskReq, "PUT");
			return CreateTask(taskResp);
		}

		public Task RemoveTask(int projectId, int storyId, int taskId)
		{
			var path = string.Format("/projects/{0}/stories/{1}/tasks/{2}", projectId, storyId, taskId);
			var taskResp = RequestPivotal<StoryTaskXmlResponse>(path, null, "DELETE");
			return CreateTask(taskResp);
		}

		public Story DeleteStory(int projectId, int storyId)
		{
				var path = string.Format("/projects/{0}/stories/{1}", projectId, storyId);
				var e = this.RequestPivotal<StoryXmlResponse>(path, null, "DELETE");

			return CreateStory(e);
		}

		public IEnumerable<Story> DeliverAllFinishedStories(int projectId)
		{
			var path = string.Format("/projects/{0}/stories/deliver_all_finished", projectId);
			return GetStories(path, "PUT");
		}

		public enum MovePositionEnum
		{
			After,
			Before
		} ;

		public Story MoveStory(int projectId, int storyId, MovePositionEnum move, int targetStoryId)
		{
			var path = string.Format("/projects/{0}/stories/{1}/moves?move\\[move\\]={2}&move\\[target\\]={3}",
				projectId,
				storyId,
				move,
				targetStoryId);
			var e = this.RequestPivotal<StoryXmlResponse>(path, null, "POST");

			return CreateStory(e);
		}

		public Story LinkStoryToExternal(int projectId, int storyId, dynamic integrationInfo)
		{
			//TODO: to be implemented
			throw new NotImplementedException();

		}
	}
}