namespace EdFi.Identity.Controllers
{
    using System.Collections.Generic;

    /// <summary>
    /// request DTO for Identity Merges
    /// </summary>
    public class PostMergeRequest
    {
        /// <summary>
        /// The first EduId for the Identity to be merged
        /// </summary>
        public string PrimaryEduId { get; set; }

        /// <summary>
        /// The second EduId for the Identity to be merged
        /// </summary>
        public string SecondaryEduId { get; set; }
    }

    public class PutMergeRequest
    {
        /// <summary>
        /// The second EduId for the Identity to be merged
        /// </summary>
        public string SecondaryEduId { get; set; }
    }

    /// <summary>
    /// Response DTO for Identity Merges
    /// </summary>
    public class MergeResponse
    {
        /// <summary>
        /// This identifier will be used in place of the provided list of identifiers.
        /// </summary>
        public string EduId { get; set; }
    }

    public class MergeResponseList
    {
        public IEnumerable<string> Identifiers { get; set; }
    }

    /*
    Merge functionality is not supported in this version of the API
    /// <summary>
    /// API for Merging Identity information
    /// </summary>
    [Description("Merge Identity identities")]
    public class MergeController : ApiController
    {
        private readonly IQueryProcessor _queryProcessor;

        public MergeController(IQueryProcessor queryProcessor)
        {
            _queryProcessor = queryProcessor;
        }

        [HttpGet]
        [ApiOperation(Summary = "Retrieve a list of all primary identies that are merged", Notes = "Individually query each merge to retrieve the details of that merge")]
        [ApiResponseMessage(Code = HttpStatusCode.OK, Message = "Successfully retrieved the list of merged identies.")]
        public MergeResponseList Get()
        {
            var query = new IdentityMerges();
            var result = new MergeResponseList
            {
                Identifiers = _queryProcessor.Process(query).EduIds
            };

            return result;
        }

        /// <summary>
        /// Retrieve all aliases for an identifier
        /// </summary>
        /// <param name="id">Any of the merged identifiers for a Identity</param>
        /// <returns>List of identifiers that map to the same Identity</returns>
        [HttpGet]
        [ApiOperation(Summary = "Retrieve all aliases for an identifier", Notes = "There may not be any aliases if the identity has not been previously merged.")]
        [ApiResponseMessage(Code = HttpStatusCode.OK, Message = "Successfully found the provided EduId.")]
        [ApiResponseMessage(Code = HttpStatusCode.NotFound, Message = "The requested EduId could not be found.")]
        public MergeResponseList Get([Description("Any of the merged identifiers for a Identity")]string id)
        {
            var query = new IdentityMerges { EduId = id };

            var result = new MergeResponseList
            {
                Identifiers = _queryProcessor.Process(query).EduIds
            };

            if (result.Identifiers.Any())
            {
                return result;
            }

            throw new HttpException(404, "The supplied EduId could not be found.");
        }

        /// <summary>
        /// Combine multiple people into one, preserving historic EduId information
        /// </summary>
        /// <param name="request">A list of EduIds</param>
        /// <returns>the current Identity information (including EduId)</returns>
        [HttpPost]
        [ApiOperation(Summary = "Combine multiple people into one, preserving historic EduId information", Notes = "A Post effectively combines two identities into one. It may be undone with a DELETE.")]
        [ApiResponseMessage(Code = HttpStatusCode.OK, Message = "Successfully merged the provided EduIds.")]
        [ApiResponseMessage(Code = HttpStatusCode.NotFound, Message = "One or more of the supplied EduIds could not be found.")]
        public MergeResponse Post([Description("EduIds to be merged")] PostMergeRequest request)
        {
            var q1 = new VerifyPerson
            {
                Identity = new Identity { EduId = request.PrimaryEduId }
            };

            var r1 = _queryProcessor.Process(q1);

            var q2 = new VerifyPerson
            {
                Identity = new Identity { EduId = request.SecondaryEduId }
            };

            var r2 = _queryProcessor.Process(q2);

            if (r1 && r2)
            {
                var command = new Merge
                {
                    PrimaryEduId = request.PrimaryEduId,
                    SecondaryEduId = request.SecondaryEduId
                };

                _queryProcessor.Process(command);

                return new MergeResponse { EduId = request.PrimaryEduId };
            }
            throw new HttpException(404, "One or more of the supplied EduIds could not be found");
        }

        /// <summary>
        /// Combine multiple People into one, preserving historic EduId information
        /// </summary>
        /// <param name="id">The primary EduId</param>
        /// <param name="request">A secondary EduId</param>
        /// <returns>the current Identity information (including EduId)</returns>
        [HttpPut]
        [ApiOperation(Summary = "Combine multiple people into one, preserving historic EduId information", Notes = "A Put effectively combines the primary and secondary identities into one. It may be undone with a DELETE.")]
        [ApiResponseMessage(Code = HttpStatusCode.OK, Message = "Successfully merged the provided Identity information.")]
        [ApiResponseMessage(Code = HttpStatusCode.NotFound, Message = "One or more of the supplied people could not be found.")]
        public MergeResponse Put(string id, [Description("Identity records to be merged")] PutMergeRequest request)
        {
            var q1 = new VerifyPerson
            {
                Identity = new Identity { EduId = id }
            };

            var r1 = _queryProcessor.Process(q1);

            var q2 = new VerifyPerson
            {
                Identity = new Identity { EduId = request.SecondaryEduId }
            };

            var r2 = _queryProcessor.Process(q2);

            if (r1 && r2)
            {
                var command = new Merge
                {
                    PrimaryEduId = id,
                    SecondaryEduId = request.SecondaryEduId
                };

                _queryProcessor.Process(command);

                return new MergeResponse { EduId = id };
            }
            throw new HttpException(404, "One or more of the supplied EduIds could not be found");
        }

        /// <summary>
        /// Un-Merge an identifier and make it a stand alone entity again
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete]
        [ApiOperation(Summary = "Un-Merge an identifier and make it a stand alone entity again", Notes = "The DELETE call effectively deletes the most recent merge on an identity.")]
        [ApiResponseMessage(Code = HttpStatusCode.OK, Message = "Successfully un-merged the provided EduId.")]
        [ApiResponseMessage(Code = HttpStatusCode.NotFound, Message = "The supplied EduId could not be found.")]
        public void Delete(string id)
        {
            var q1 = new VerifyPerson
            {
                Identity = new Identity { EduId = id }
            };

            var r1 = _queryProcessor.Process(q1);

            if (r1)
            {
                var command = new Split { EduId = id };

                _queryProcessor.Process(command);
            }
            else
            {
                throw new HttpException(404, "The supplied EduId could not be found.");
            }
        }
     
    }
     * * */
}
