using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using RestSharp;

namespace Mirage;

public static class API
{
    private static RestClient _client = new RestClient("https://api.mirageml.com/");

    public static class Public
    {
        /// <summary>
        /// Returns the public 3D assets created on Mirage
        /// </summary>
        /// <param name="page">The paginated page to obtain data</param>
        /// <param name="filter">A text filter to search projects based on prompt</param>
        /// <param name="projectId">Project to query</param>
        public static async Task<SPublicDataset?> PublicDataset([Optional] string page, [Optional] string filter,
            [Optional] string projectId)
        {
            RestRequest req = new RestRequest("/texture-mesh/public-projects");
            if (page != String.Empty) req.AddQueryParameter("page", page);
            if (filter != String.Empty) req.AddQueryParameter("filter", filter);
            if (projectId != String.Empty) req.AddQueryParameter("projectID", projectId);
            var res = await _client.GetAsync<SPublicDataset>(req);
            return res;
        }

        /// <summary>
        /// Returns an array of the public 3D Asset projects
        /// </summary>
        /// <param name="page">The page number, 30 assets per page</param>
        public static async Task<RGetPublicProjects?> GetPublicProjects([Optional] string page)
        {
            RestRequest req = new RestRequest("/texture-mesh/public-projects");
            if (page != String.Empty) req.AddQueryParameter("page", page);
            var res = await _client.GetAsync<RGetPublicProjects>(req);
            return res;
        }


        #region PublicDataset

        public struct Datum
        {
            public string id { get; set; }
            public DateTime created_at { get; set; }
            public string user_id { get; set; }
            public object run_id { get; set; }
            public string project_id { get; set; }
            public string mesh_prompt { get; set; }
            public string texture_prompt { get; set; }
            public double metallic { get; set; }
            public double roughness { get; set; }
            public bool generate_maps { get; set; }
            public string mesh_algorithm { get; set; }
            public string guidance { get; set; }
            public int seed { get; set; }
            public int train_iterations { get; set; }
            public int votes { get; set; }
            public bool @public { get; set; }
            public string negative_prompt { get; set; }
            public int negative_votes { get; set; }
            public string stable_diffusion_checkpoint { get; set; }
            public string inference_run_id { get; set; }
            public string style { get; set; }
            public string polygon { get; set; }
            public Urls urls { get; set; }
            public bool enable_remix { get; set; }
            public string mesh_url { get; set; }
            public string glb_url { get; set; }
            public string png_url { get; set; }
        }

        public struct SPublicDataset
        {
            public List<Datum> data { get; set; }
        }

        public struct Urls
        {
            public string fbx { get; set; }
            public object gif { get; set; }
            public string glb { get; set; }
            public string mtl { get; set; }
            public string obj { get; set; }
            public string png { get; set; }
            public string gltf { get; set; }
        }

        #endregion

        #region GetPublicProjects

        public record GetPublicProjectsData(
            string id,
            DateTime created_at,
            string user_id,
            object run_id,
            string project_id,
            string mesh_prompt,
            string texture_prompt,
            double metallic,
            double roughness,
            bool generate_maps,
            string mesh_algorithm,
            string guidance,
            int seed,
            int train_iterations,
            int votes,
            bool @public,
            object negative_prompt,
            int negative_votes,
            string stable_diffusion_checkpoint,
            string inference_run_id,
            string style,
            string polygon,
            string mesh_url,
            string glb_url,
            string png_url
        );

        public record RGetPublicProjects(
            IReadOnlyList<GetPublicProjectsData> data
        );

        #endregion
    }

    public static class Private
    {
        public static class StableDiffusion
        {
            /// <summary>
            /// Returns an array of the stable diffusion projects created
            /// </summary>
            /// <param name="authKey">Authorization Key</param>
            /// <param name="apiKey">x-api-key</param>
            /// <exception cref="ArgumentException">Throws if either keys are empty or null</exception>
            public static async Task<SGetProjects> GetProjects(SClientAuthorization authorization)
            {
                RestRequest req = new RestRequest("/stable-diffusion/projects");
                AddAuthHeaders(authorization, ref req);

                return await _client.GetAsync<SGetProjects>(req);
            }

            /// <summary>
            /// Returns the stable diffusion project matching the projectId
            /// </summary>
            /// <param name="authorization">Authorization of client</param>
            /// <param name="projectId">Project ID. Cannot be empty or null</param>
            /// <returns>Completed Request</returns>
            /// <exception cref="ArgumentException">Throws if any inputs are empty or null</exception>
            public static async Task<SGetProject> GetProject(SClientAuthorization authorization, string projectId)
            {
                if (projectId == String.Empty) throw new ArgumentException("projectId cannot be empty!");
                RestRequest req = new RestRequest("/stable-diffusion/project");
                AddAuthHeaders(authorization, ref req);
                req.AddQueryParameter("projectId", projectId);
                return await _client.GetAsync<SGetProject>(req);
            }

            /// <summary>
            /// Creates a stable diffusion project.
            /// </summary>
            /// <param name="authorization">Authorization of the client.</param>
            /// <returns>Completed request</returns>
            public static async Task<SCreateProject> CreateProject(SClientAuthorization authorization)
            {
                RestRequest req = new RestRequest("/stable-diffusion/project");
                AddAuthHeaders(authorization, ref req);
                return await _client.PostAsync<SCreateProject>(req);
            }

            /// <summary>
            /// Gets a stable diffusion run. A run is an instance of a stable diffusion image generation. Returns the run data and generated images for the run.
            /// </summary>
            /// <param name="authorization">Authorization for the client</param>
            /// <param name="runId">ID of the run</param>
            /// <returns>The run data and generated images for the run</returns>
            public static async Task<SGetRun> GetRun(SClientAuthorization authorization, string runId)
            {
                if (runId == String.Empty) throw new ArgumentException("runId cannot be empty!");

                RestRequest req = new RestRequest("/stable-diffusion/run");
                AddAuthHeaders(authorization, ref req);
                return await _client.GetAsync<SGetRun>(req);
            }

            /// <summary>
            /// Run stable diffusion image generation with the requested parameters
            /// </summary>
            /// <param name="authorization">Authorization of client</param>
            /// <param name="body">Body which includes data for the run</param>
            /// <returns>Completed request</returns>
            /// <exception cref="ArgumentException">Throws if either input is null</exception>
            public static async Task<SCreateRun> CreateRun(SClientAuthorization authorization, SCreateRunBody body)
            {
                if (body.Equals(null)) throw new ArgumentException("Body cannot be null!");

                RestRequest req = new RestRequest("/stable-diffusion/run");
                AddAuthHeaders(authorization, ref req);
                req.AddBody(body);
                return await _client.PostAsync<SCreateRun>(req);
            }


            #region Structs

            public struct SGetProjects
            {
                public Node node { get; set; }

                public struct TextualInversionRunsCollection
                {
                    public List<Edge> edges { get; set; }
                }

                public struct Edge
                {
                    public Node node { get; set; }
                }

                public struct Node
                {
                    public string id { get; set; }
                    public string init_word { get; set; }
                    public DateTime created_at { get; set; }
                    public string finetune_job_status { get; set; }
                    public TextualInversionRunsCollection textual_inversion_runsCollection { get; set; }
                    public string width { get; set; }
                    public string height { get; set; }
                    public string prompt { get; set; }
                    public string status { get; set; }
                    public bool init_image { get; set; }
                    public string num_images { get; set; }
                    public double guidance_scale { get; set; }
                    public double prompt_strength { get; set; }
                    public string num_inference_steps { get; set; }
                }
            }

            public struct SGetProject
            {
                public string id { get; set; }
                public string init_word { get; set; }
                public DateTime created_at { get; set; }
                public string finetune_job_status { get; set; }
                public TextualInversionRunsCollection textual_inversion_runsCollection { get; set; }

                public struct Edge
                {
                    public Node node { get; set; }
                }

                public struct Node
                {
                    public string id { get; set; }
                    public string width { get; set; }
                    public string height { get; set; }
                    public string prompt { get; set; }
                    public string status { get; set; }
                    public DateTime created_at { get; set; }
                    public bool init_image { get; set; }
                    public string num_images { get; set; }
                    public double guidance_scale { get; set; }
                    public double prompt_strength { get; set; }
                    public string num_inference_steps { get; set; }
                }

                public struct TextualInversionRunsCollection
                {
                    public List<Edge> edges { get; set; }
                }
            }

            public struct SCreateProject
            {
                public int statusCode { get; set; }
                public string project_id { get; set; }
            }

            public struct SGetRun
            {
                public string id { get; set; }
                public string width { get; set; }
                public string height { get; set; }
                public string prompt { get; set; }
                public string status { get; set; }
                public DateTime created_at { get; set; }
                public string num_images { get; set; }
                public string project_id { get; set; }
                public double guidance_scale { get; set; }
                public double prompt_strength { get; set; }
                public string num_inference_steps { get; set; }
                public List<string> output_images { get; set; }
            }

            public struct SCreateRunBody
            {
                [JsonPropertyName("project-id")] public string projectId;
                [JsonPropertyName("prompt")] public string prompt;
                [JsonPropertyName("project-id")] public string numImaged;
            }

            public struct SCreateRun
            {
                [JsonPropertyName("statusCode")] public int statusCode { get; set; }

                [JsonPropertyName("run_id")] public string run_id { get; set; }
            }

            #endregion
        }

        public static class Dreambooth
        {
            /// <summary>
            /// Returns an array of the dreambooth projects
            /// </summary>
            /// <param name="authorization">Authorization for the client</param>
            /// <returns></returns>
            public static async Task<SGetProjects> GetProjects(SClientAuthorization authorization)
            {
                RestRequest req = new RestRequest("/dreambooth/projects");
                AddAuthHeaders(authorization, ref req);
                return await _client.GetAsync<SGetProjects>(req);
            }

            /// <summary>
            /// Returns the dreambooth project
            /// </summary>
            /// <param name="authorization">Authorization of the client</param>
            /// <param name="projectId">Project ID to get</param>
            /// <returns>Completed request</returns>
            /// <exception cref="ArgumentException">Throws if project is null or empty</exception>
            public static async Task<SGetProject> GetProject(SClientAuthorization authorization, string projectId)
            {
                if (projectId == String.Empty) throw new ArgumentException("Project Id cannot be null or empty!");
                RestRequest req = new RestRequest("/dreambooth/project");
                AddAuthHeaders(authorization, ref req);
                req.AddQueryParameter("project-id", projectId);
                return await _client.GetAsync<SGetProject>(req);
            }

            /// <summary>
            /// Creates a Dreambooth project
            /// </summary>
            /// <param name="authorization">Authorization of the client</param>
            /// <param name="body">Body of post request</param>
            /// <returns>Completed request</returns>
            public static async Task<SCreateProject> CreateProject(SClientAuthorization authorization,
                SCreateProjectBody body)
            {
                RestRequest req = new RestRequest("/dreambooth/project");
                AddAuthHeaders(authorization, ref req);
                req.AddBody(body);
                return await _client.PostAsync<SCreateProject>(req);
            }

            /// <summary>
            /// Gets a dreambooth run. A run is an instance of a dreambooth image generation.
            /// Returns the run data and generated images for the run.
            /// </summary>
            /// <param name="authorization">Client authorization</param>
            /// <param name="runId">The id of the run</param>
            /// <returns>Completed request</returns>
            public static async Task<SGetRun> GetRun(SClientAuthorization authorization, string runId)
            {
                RestRequest req = new RestRequest("/dreambooth/run");
                AddAuthHeaders(authorization, ref req);

                req.AddQueryParameter("run-id", runId);
                return await _client.GetAsync<SGetRun>(req);
            }

            /// <summary>
            /// Run dreambooth image generation with an optional initial image
            /// </summary>
            /// <param name="authorization"></param>
            /// <param name="body"></param>
            /// <returns></returns>
            public static async Task<SCreateRun> CreateRun(SClientAuthorization authorization, SCreateRunBody body)
            {
                RestRequest req = new RestRequest("/dreambooth/run");
                AddAuthHeaders(authorization, ref req);
                req.AddBody(body);
                return await _client.PostAsync<SCreateRun>(req);
            }

            #region structs

            public struct SGetProjects
            {
                [JsonProperty("node")] [JsonPropertyName("node")]
                public StableDiffusion.SGetProject.Node Node;

                public struct TextualInversionRunsCollection
                {
                    [JsonProperty("edges")] [JsonPropertyName("edges")]
                    public List<Edge> Edges;
                }

                public struct Edge
                {
                    [JsonProperty("node")] [JsonPropertyName("node")]
                    public StableDiffusion.SGetProject.Node Node;
                }
            }


            public struct SGetProject
            {
                [JsonProperty("node")] [JsonPropertyName("node")]
                public SNode Node;

                public struct TextualInversionRunsCollection
                {
                    [JsonProperty("edges")] [JsonPropertyName("edges")]
                    public List<Edge> Edges;
                }

                public struct Edge
                {
                    [JsonProperty("node")] [JsonPropertyName("node")]
                    public SNode Node;
                }

                public struct SNode
                {
                    [JsonProperty("id")] [JsonPropertyName("id")]
                    public string Id;

                    [JsonProperty("init_word")] [JsonPropertyName("init_word")]
                    public string InitWord;

                    [JsonProperty("train_iterations")] [JsonPropertyName("train_iterations")]
                    public int TrainIterations;

                    [JsonProperty("created_at")] [JsonPropertyName("created_at")]
                    public DateTime CreatedAt;

                    [JsonProperty("finetune_job_status")] [JsonPropertyName("finetune_job_status")]
                    public string FinetuneJobStatus;

                    [JsonProperty("textual_inversion_runsCollection")]
                    [JsonPropertyName("textual_inversion_runsCollection")]
                    public TextualInversionRunsCollection TextualInversionRunsCollection;

                    [JsonProperty("width")] [JsonPropertyName("width")]
                    public string Width;

                    [JsonProperty("height")] [JsonPropertyName("height")]
                    public string Height;

                    [JsonProperty("prompt")] [JsonPropertyName("prompt")]
                    public string Prompt;

                    [JsonProperty("status")] [JsonPropertyName("status")]
                    public string Status;

                    [JsonProperty("init_image")] [JsonPropertyName("init_image")]
                    public bool InitImage;

                    [JsonProperty("num_images")] [JsonPropertyName("num_images")]
                    public string NumImages;

                    [JsonProperty("guidance_scale")] [JsonPropertyName("guidance_scale")]
                    public double GuidanceScale;

                    [JsonProperty("prompt_strength")] [JsonPropertyName("prompt_strength")]
                    public double PromptStrength;

                    [JsonProperty("num_inference_steps")] [JsonPropertyName("num_inference_steps")]
                    public string NumInferenceSteps;
                }
            }


            public struct SCreateProjectBody
            {
                [JsonProperty("init-word")]
                [JsonPropertyName("init-word")]
                public string InitWord { get; set; }

                [JsonProperty("train-iterations")]
                [JsonPropertyName("train-iterations")]
                public string TrainIterations { get; set; }

                [JsonProperty("links")]
                [JsonPropertyName("links")]
                public string Links { get; set; }

                [JsonProperty("files")]
                [JsonPropertyName("files")]
                public FileStream[] Files { get; set; }
            }

            public struct SCreateProject
            {
                [JsonProperty("statusCode")] [JsonPropertyName("statusCode")]
                public int StatusCode;

                [JsonProperty("projectId")] [JsonPropertyName("projectId")]
                public string ProjectId;

                [JsonProperty("uploadedFileLength")] [JsonPropertyName("uploadedFileLength")]
                public int UploadedFileLength;
            }

            public struct SGetRun
            {
                [JsonProperty("id")] [JsonPropertyName("id")]
                public string Id;

                [JsonProperty("width")] [JsonPropertyName("width")]
                public string Width;

                [JsonProperty("height")] [JsonPropertyName("height")]
                public string Height;

                [JsonProperty("prompt")] [JsonPropertyName("prompt")]
                public string Prompt;

                [JsonProperty("status")] [JsonPropertyName("status")]
                public string Status;

                [JsonProperty("created_at")] [JsonPropertyName("created_at")]
                public DateTime CreatedAt;

                [JsonProperty("num_images")] [JsonPropertyName("num_images")]
                public string NumImages;

                [JsonProperty("project_id")] [JsonPropertyName("project_id")]
                public string ProjectId;

                [JsonProperty("guidance_scale")] [JsonPropertyName("guidance_scale")]
                public double GuidanceScale;

                [JsonProperty("prompt_strength")] [JsonPropertyName("prompt_strength")]
                public double PromptStrength;

                [JsonProperty("num_inference_steps")] [JsonPropertyName("num_inference_steps")]
                public string NumInferenceSteps;

                [JsonProperty("output_images")] [JsonPropertyName("output_images")]
                public List<string> OutputImages;
            }

            public struct SCreateRunBody
            {
                [JsonProperty("project-id")] [JsonPropertyName("project-id")]
                public string ProjectId;

                [JsonProperty("prompt")] [JsonPropertyName("prompt")]
                public string Prompt;

                [JsonProperty("num-images")] [JsonPropertyName("num-images")]
                public string NumImages;

                [JsonProperty("prompt-strength")] [JsonPropertyName("prompt-strength")]
                public string PromptStrength;

                [JsonProperty("width")] [JsonPropertyName("width")]
                public int Width;

                [JsonProperty("height")] [JsonPropertyName("height")]
                public int Height;

                [JsonProperty("num-inference-steps")] [JsonPropertyName("num-inference-steps")]
                public int NumInferenceSteps;

                [JsonProperty("guidance-scale")] [JsonPropertyName("guidance-scale")]
                public string GuidanceScale;

                [JsonProperty("files")] [JsonPropertyName("files")]
                public FileStream[] Files;
            }

            public struct SCreateRun
            {
                [JsonProperty("statusCode")] [JsonPropertyName("statusCode")]
                public int StatusCode;

                [JsonProperty("run_id")] [JsonPropertyName("run_id")]
                public string RunId;
            }

            #endregion
        }

        public static class TextureMesh
        {
            /// <summary>
            /// Returns an array of the texture mesh projects
            /// </summary>
            /// <param name="authorization">Client Authorization</param>
            /// <param name="cursor">Starting cursor</param>
            /// <param name="sort">The sort direction. Enum: ascending, descending</param>
            /// <returns></returns>
            public static async Task<SGetProjects> GetProjects(SClientAuthorization authorization, string cursor,
                string sort = "descending")
            {
                if (sort != "descending" || sort != "ascending")
                    throw new ArgumentException("Sort can only be either ascending or descending");
                RestRequest req = new RestRequest("/texture-mesh/projects");
                AddAuthHeaders(authorization, ref req);

                if (cursor != String.Empty) req.AddQueryParameter("cursor", cursor);
                req.AddQueryParameter("sort", sort.ToString().ToLower());
                return await _client.GetAsync<SGetProjects>(req);
            }

            /// <summary>
            /// Returns the texture mesh project
            /// </summary>
            /// <param name="authorization">Authorization of the client</param>
            /// <param name="projectId">Id of the project</param>
            /// <returns></returns>
            public static async Task<SGetProject> GetProject(SClientAuthorization authorization, string projectId)
            {
                RestRequest req = new RestRequest("/texture-mesh/project");
                AddAuthHeaders(authorization, ref req);
                req.AddQueryParameter("project-id", projectId);

                return await _client.GetAsync<SGetProject>(req);
            }

            /// <summary>
            /// Create a mesh with prompt and mesh algorithm
            /// </summary>
            /// <param name="authorization">Client authorization</param>
            /// <param name="prompt">The input prompt</param>
            /// <returns></returns>
            public static async Task<SCreateProject> CreateProject(SClientAuthorization authorization,
                SCreateProjectBody prompt)
            {
                RestRequest req = new RestRequest("/texture-mesh/project/create");
                AddAuthHeaders(authorization, ref req);
                req.AddBody(prompt);
                return await _client.PostAsync<SCreateProject>(req);
            }

            /// <summary>
            /// Create a mesh by uploading the .obj file
            /// </summary>
            /// <param name="authorization">Authorization of client</param>
            /// <param name="body">Input prompt and files</param>
            /// <returns></returns>
            public static async Task<SCreateProject> UploadProject(SClientAuthorization authorization,
                SUploadProjectBody body)
            {
                RestRequest req = new RestRequest("/texture-mesh/project/upload");
                AddAuthHeaders(authorization, ref req);
                req.AddBody(body);
                return await _client.PostAsync<SCreateProject>(req);
            }

            /// <summary>
            /// Gets a TextureMesh run. A run is an instance of a texture mesh creation. Returns the run data and generated textures.
            /// </summary>
            /// <param name="authorization">Authorization of client</param>
            /// <param name="runId">The id of the run</param>
            /// <returns></returns>
            public static async Task<SGetRun> GetRun(SClientAuthorization authorization, string runId)
            {
                RestRequest req = new RestRequest("/texture-mesh/run");
                AddAuthHeaders(authorization, ref req);
                req.AddQueryParameter("run-id", runId);
                return await _client.GetAsync<SGetRun>(req);
            }

            /// <summary>
            /// Runs a texture generation task with the requested parameters. Will be associated with mesh created
            /// </summary>
            /// <param name="authorization">Authorization of client</param>
            /// <param name="body">Body data</param>
            /// <returns></returns>
            public static async Task<SCreateRun> CreateRun(SClientAuthorization authorization, SCreateRunBody body)
            {
                RestRequest req = new RestRequest("/texture-mesh/run");
                AddAuthHeaders(authorization, ref req);
                req.AddBody(body);
                return await _client.PostAsync<SCreateRun>(req);
            }

            #region structs

            public struct SGetProjects
            {
                [JsonProperty("pageInfo")] [JsonPropertyName("pageInfo")]
                public SPageInfo PageInfo;

                [JsonProperty("data")] [JsonPropertyName("data")]
                public List<Datum> Data;

                public struct SPageInfo
                {
                    [JsonProperty("endCursor")] [JsonPropertyName("endCursor")]
                    public string EndCursor;

                    [JsonProperty("__typename")] [JsonPropertyName("__typename")]
                    public string Typename;

                    [JsonProperty("hasNextPage")] [JsonPropertyName("hasNextPage")]
                    public bool HasNextPage;

                    [JsonProperty("startCursor")] [JsonPropertyName("startCursor")]
                    public string StartCursor;
                }

                public struct TextMeshRunsCollection
                {
                    [JsonProperty("edges")] [JsonPropertyName("edges")]
                    public List<Edge> Edges;

                    [JsonProperty("__typename")] [JsonPropertyName("__typename")]
                    public string Typename;
                }

                public struct SNode
                {
                    [JsonProperty("id")] [JsonPropertyName("id")]
                    public string Id;

                    [JsonProperty("type")] [JsonPropertyName("type")]
                    public string Type;

                    [JsonProperty("prompt")] [JsonPropertyName("prompt")]
                    public string Prompt;

                    [JsonProperty("status")] [JsonPropertyName("status")]
                    public string Status;

                    [JsonProperty("__typename")] [JsonPropertyName("__typename")]
                    public string Typename;

                    [JsonProperty("created_at")] [JsonPropertyName("created_at")]
                    public DateTime CreatedAt;

                    [JsonProperty("text_mesh_runsCollection")] [JsonPropertyName("text_mesh_runsCollection")]
                    public TextMeshRunsCollection TextMeshRunsCollection;

                    [JsonProperty("meshUrl")] [JsonPropertyName("meshUrl")]
                    public string MeshUrl;

                    [JsonProperty("color_map")] [JsonPropertyName("color_map")]
                    public bool ColorMap;

                    [JsonProperty("init_image")] [JsonPropertyName("init_image")]
                    public bool InitImage;

                    [JsonProperty("num_outputs")] [JsonPropertyName("num_outputs")]
                    public string NumOutputs;

                    [JsonProperty("square_func")] [JsonPropertyName("square_func")]
                    public string SquareFunc;

                    [JsonProperty("generate_maps")] [JsonPropertyName("generate_maps")]
                    public bool GenerateMaps;

                    [JsonProperty("make_tileable")] [JsonPropertyName("make_tileable")]
                    public bool MakeTileable;

                    [JsonProperty("prompt_strength")] [JsonPropertyName("prompt_strength")]
                    public double PromptStrength;

                    [JsonProperty("super_resolution")] [JsonPropertyName("super_resolution")]
                    public string SuperResolution;
                }

                public struct Edge
                {
                    [JsonProperty("node")] [JsonPropertyName("node")]
                    public SNode Node;

                    [JsonProperty("__typename")] [JsonPropertyName("__typename")]
                    public string Typename;
                }

                public struct Datum
                {
                    [JsonProperty("node")] [JsonPropertyName("node")]
                    public SNode Node;

                    [JsonProperty("cursor")] [JsonPropertyName("cursor")]
                    public string Cursor;

                    [JsonProperty("__typename")] [JsonPropertyName("__typename")]
                    public string Typename;
                }
            }

            public struct SGetProject
            {
                [JsonProperty("node")] [JsonPropertyName("node")]
                public SNode Node;

                public struct TextMeshRunsCollection
                {
                    [JsonProperty("edges")] [JsonPropertyName("edges")]
                    public List<Edge> Edges;
                }

                public struct Edge
                {
                    [JsonProperty("node")] [JsonPropertyName("node")]
                    public SNode Node;
                }

                public struct SNode
                {
                    [JsonProperty("id")] [JsonPropertyName("id")]
                    public string Id;

                    [JsonProperty("type")] [JsonPropertyName("type")]
                    public string Type;

                    [JsonProperty("prompt")] [JsonPropertyName("prompt")]
                    public string Prompt;

                    [JsonProperty("status")] [JsonPropertyName("status")]
                    public string Status;

                    [JsonProperty("created_at")] [JsonPropertyName("created_at")]
                    public DateTime CreatedAt;

                    [JsonProperty("text_mesh_runsCollection")] [JsonPropertyName("text_mesh_runsCollection")]
                    public TextMeshRunsCollection TextMeshRunsCollection;

                    [JsonProperty("color_map")] [JsonPropertyName("color_map")]
                    public bool ColorMap;

                    [JsonProperty("init_image")] [JsonPropertyName("init_image")]
                    public bool InitImage;

                    [JsonProperty("num_outputs")] [JsonPropertyName("num_outputs")]
                    public string NumOutputs;

                    [JsonProperty("square_func")] [JsonPropertyName("square_func")]
                    public string SquareFunc;

                    [JsonProperty("generate_maps")] [JsonPropertyName("generate_maps")]
                    public bool GenerateMaps;

                    [JsonProperty("make_tileable")] [JsonPropertyName("make_tileable")]
                    public bool MakeTileable;

                    [JsonProperty("prompt_strength")] [JsonPropertyName("prompt_strength")]
                    public double PromptStrength;

                    [JsonProperty("super_resolution")] [JsonPropertyName("super_resolution")]
                    public string SuperResolution;
                }
            }

            public struct SCreateProjectBody
            {
                [JsonProperty("prompt")] [JsonPropertyName("prompt")]
                public string Prompt;
            }

            public struct SCreateProject
            {
                [JsonProperty("statusCode")] [JsonPropertyName("statusCode")]
                public int StatusCode;

                [JsonProperty("project_id")] [JsonPropertyName("project_id")]
                public string ProjectId;
            }

            public struct SUploadProjectBody
            {
                [JsonProperty("prompt")] [JsonPropertyName("prompt")]
                public string Prompt;

                [JsonProperty("files")] [JsonPropertyName("files")]
                public FileStream[] Files;
            }

            public struct SGetRun
            {
                [JsonProperty("id")] [JsonPropertyName("id")]
                public string Id;

                [JsonProperty("prompt")] [JsonPropertyName("prompt")]
                public string Prompt;

                [JsonProperty("status")] [JsonPropertyName("status")]
                public string Status;

                [JsonProperty("color_map")] [JsonPropertyName("color_map")]
                public bool ColorMap;

                [JsonProperty("created_at")] [JsonPropertyName("created_at")]
                public DateTime CreatedAt;

                [JsonProperty("init_image")] [JsonPropertyName("init_image")]
                public bool InitImage;

                [JsonProperty("num_outputs")] [JsonPropertyName("num_outputs")]
                public string NumOutputs;

                [JsonProperty("square_func")] [JsonPropertyName("square_func")]
                public string SquareFunc;

                [JsonProperty("generate_maps")] [JsonPropertyName("generate_maps")]
                public bool GenerateMaps;

                [JsonProperty("make_tileable")] [JsonPropertyName("make_tileable")]
                public bool MakeTileable;

                [JsonProperty("prompt_strength")] [JsonPropertyName("prompt_strength")]
                public double PromptStrength;

                [JsonProperty("super_resolution")] [JsonPropertyName("super_resolution")]
                public string SuperResolution;

                [JsonProperty("textures")] [JsonPropertyName("textures")]
                public string Textures;
            }

            public struct SCreateRunBody
            {
                [JsonProperty("project-id")] [JsonPropertyName("project-id")]
                public string ProjectId;

                [JsonProperty("prompt")] [JsonPropertyName("prompt")]
                public string Prompt;

                [JsonProperty("generate-maps")] [JsonPropertyName("generate-maps")]
                public bool GenerateMaps;

                [JsonProperty("image-type")] [JsonPropertyName("image-type")]
                public string ImageType;

                [JsonProperty("super-resolution")] [JsonPropertyName("super-resolution")]
                public int SuperResolution;

                [JsonProperty("square-func")] [JsonPropertyName("square-func")]
                public string SquareFunc;

                [JsonProperty("make-tileable")] [JsonPropertyName("make-tileable")]
                public bool MakeTileable;

                [JsonProperty("files")] [JsonPropertyName("files")]
                public FileStream[] Files;
            }

            public struct SCreateRun
            {
                [JsonProperty("statusCode")] [JsonPropertyName("statusCode")]
                public int StatusCode;

                [JsonProperty("run_id")] [JsonPropertyName("run_id")]
                public string RunId;
            }

            #endregion
        }

        /// <summary>
        /// Authorizes the user. If the request does not return a OK status, then we assume the client is not authorized
        /// </summary>
        /// <param name="authorization">Client Authorization</param>
        /// <returns>True if  request returns OK</returns>
        public static async Task<bool> AuthorizeUser(SClientAuthorization authorization)
        {
            RestRequest req = new RestRequest("/dreambooth/projects");
            AddAuthHeaders(authorization, ref req);
            return (await _client.GetAsync(req)).StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Checks the auth key and api key to make sure they are not null or empty.
        /// </summary>
        /// <param name="authorization">Authorization for client</param>
        /// <exception cref="ArgumentException">Throws if any inputs are invalid</exception>
        private static void CheckInputs(SClientAuthorization authorization)
        {
            if (authorization.AuthorizationToken == String.Empty)
                throw new ArgumentException("Authorization key is empty!");
            if (authorization.ApiKey == String.Empty) throw new ArgumentException("api key is empty!");
        }

        /// <summary>
        /// Adds the authorization headers to the entered request. Calls the CheckInputs() Method
        /// </summary>
        /// <param name="authorization">Authorization of client</param>
        /// <param name="req">Ref to the RestRequest. May not be null</param>
        /// <exception cref="ArgumentException">Is thrown if any of the inputs are empty or null</exception>
        private static void AddAuthHeaders(SClientAuthorization authorization, ref RestRequest req)
        {
            CheckInputs(authorization);
            req.AddHeaders(new List<KeyValuePair<string, string>>()
            {
                new("Authorization", authorization.AuthorizationToken),
                new("x-api-key", authorization.ApiKey),
            });
        }

        #region Structs

        public struct SClientAuthorization
        {
            public readonly string ApiKey;
            public readonly string AuthorizationToken;

            public SClientAuthorization(string apiKey, string authToken)
            {
                ApiKey = apiKey;
                AuthorizationToken = authToken;
            }
        }

        #endregion
    }
}

#if DEBUG
public class Class
{
    public static void Main(string[] args)
    {
    }
}
#endif