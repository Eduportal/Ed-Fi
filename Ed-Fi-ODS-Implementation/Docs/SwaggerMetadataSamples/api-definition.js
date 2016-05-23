{
	"basePath": "http://localhost/EdFi.Ods.WebService/api",
	"resourcePath": "/academicHonorsTypes",
	"apis": [{
		"path": "/academicHonorsTypes",
		"description": "A designation of the type of academic distinctions earned by or awarded to the student.  This GET operation retrieves resources based on explicit criteria, with paging and sorting capabilities.",
		"operations": [{
			"httpMethod": "GET",
			"nickname": "getacademicHonorsTypes",
			"summary": "A designation of the type of academic distinctions earned by or awarded to the student.  This GET operation retrieves resources based on explicit criteria, with paging and sorting capabilities.",
			"parameters": [{
				"name": "offset",
				"description": "Indicates how many items should be skipped before returning results.",
				"paramType": "query",
				"allowMultiple": false,
				"required": false,
				"dataType": "int"
			},
			{
				"name": "limit",
				"description": "Indicates the maxmimum number of items that should be returned in the results (defaults to 25).",
				"paramType": "query",
				"allowMultiple": false,
				"required": false,
				"dataType": "int"
			},
			{
				"name": "sortBy",
				"description": "Indicates the property by which the results should be sorted.",
				"paramType": "query",
				"allowMultiple": false,
				"required": false,
				"dataType": "string"
			},
			{
				"name": "sortDirection",
				"description": "Indicates the direction ('ascending' or 'descending') in which the results should be sorted.",
				"paramType": "query",
				"allowMultiple": false,
				"required": false,
				"dataType": "string"
			},
			{
				"name": "q",
				"description": "(Reserved for future use.) Contains a specialized query expression to be applied to the results.",
				"paramType": "query",
				"allowMultiple": false,
				"required": false,
				"dataType": "string"
			},
			{
				"name": "academicHonorsTypeId",
				"description": "(Key Field) Key for AcademicHonor",
				"paramType": "query",
				"allowMultiple": false,
				"required": false,
				"dataType": "string"
			},
			{
				"name": "codeValue",
				"description": "Code for academic honors type.",
				"paramType": "query",
				"allowMultiple": false,
				"required": false,
				"dataType": "string"
			},
			{
				"name": "description",
				"description": "",
				"paramType": "query",
				"allowMultiple": false,
				"required": false,
				"dataType": "string"
			},
			{
				"name": "If-None-Match",
				"description": "The previously returned ETag header value, used here to prevent the unnecessary data transfer of an unchanged resource.",
				"paramType": "header",
				"allowMultiple": false,
				"required": false,
				"dataType": "string"
			}],
			"errorResponses": []
		}]
	},
	{
		"path": "/academicHonorsTypes",
		"description": "Processes the contained resource, creating or updating as necessary.",
		"operations": [{
			"httpMethod": "POST",
			"nickname": "postacademicHonorsTypes",
			"summary": "Processes the contained resource, creating or updating as necessary.",
			"parameters": [{
				"paramType": "body",
				"allowMultiple": false,
				"required": false,
				"dataType": "AcademicHonorsTypesPost"
			}],
			"errorResponses": []
		}]
	},
	{
		"path": "/academicHonorsTypes/{Id}",
		"description": "Updates the specified resource.",
		"operations": [{
			"httpMethod": "PUT",
			"nickname": "putacademicHonorsTypesId",
			"summary": "Updates the specified resource.",
			"parameters": [{
				"name": "Id",
				"description": "The unique identifier of the resource.",
				"paramType": "path",
				"allowMultiple": false,
				"required": true,
				"dataType": "string"
			},
			{
				"name": "If-Match",
				"description": "The ETag header value used to prevent the PUT from overwriting another consumer's changes.",
				"paramType": "header",
				"allowMultiple": false,
				"required": false,
				"dataType": "string"
			},
			{
				"paramType": "body",
				"allowMultiple": false,
				"required": false,
				"dataType": "AcademicHonorsTypePut"
			}],
			"errorResponses": []
		}]
	},
	{
		"path": "/academicHonorsTypes/{Id}",
		"description": "Updates an existing resource using a partial representation.",
		"operations": [{
			"httpMethod": "PATCH",
			"nickname": "patchacademicHonorsTypesId",
			"summary": "Updates an existing resource using a partial representation.",
			"parameters": [{
				"name": "Id",
				"description": "The identifier of the resource.",
				"paramType": "path",
				"allowMultiple": false,
				"required": true,
				"dataType": "string"
			},
			{
				"name": "AcademicHonorsTypePatch",
				"description": "The partial resource representation to apply to the specified resource.",
				"paramType": "body",
				"allowMultiple": false,
				"required": true,
				"dataType": "string"
			},
			{
				"name": "If-Match",
				"description": "The ETag header value used to prevent the PATCH from overwriting another consumer's changes in an unexpected way.",
				"paramType": "header",
				"allowMultiple": false,
				"required": true,
				"dataType": "string"
			}],
			"errorResponses": []
		}]
	},
	{
		"path": "/academicHonorsTypes/{Id}",
		"description": "Deletes the specified resource.",
		"operations": [{
			"httpMethod": "DELETE",
			"nickname": "deleteacademicHonorsTypesId",
			"summary": "Deletes the specified resource.",
			"parameters": [{
				"name": "Id",
				"description": "The identifier of the AcademicHonorsType resource to delete.",
				"paramType": "path",
				"allowMultiple": false,
				"required": true,
				"dataType": "int"
			},
			{
				"name": "If-Match",
				"description": "The ETag header value used to prevent the DELETE from removing a resource modified by another consumer.",
				"paramType": "header",
				"allowMultiple": false,
				"required": false,
				"dataType": "string"
			},
			{
				"paramType": "body",
				"allowMultiple": false,
				"required": false,
				"dataType": "AcademicHonorsTypeDelete"
			}],
			"errorResponses": []
		}]
	},
	{
		"path": "/academicHonorsTypes/{Ids}",
		"description": "Gets the specified resource(s) by resource identifier.",
		"operations": [{
			"httpMethod": "GET",
			"nickname": "getacademicHonorsTypesIds",
			"summary": "Gets the specified resource(s) by resource identifier.",
			"parameters": [{
				"name": "Ids",
				"description": "The identifier (or comma-separated list of identifiers) of the AcademicHonorsType resource to retrieve.",
				"paramType": "path",
				"allowMultiple": false,
				"required": true,
				"dataType": "string"
			},
			{
				"name": "If-None-Match",
				"description": "The previously returned ETag header value, used here to prevent the unnecessary data transfer of an unchanged resource.",
				"paramType": "header",
				"allowMultiple": false,
				"required": false,
				"dataType": "string"
			}],
			"errorResponses": []
		}]
	}],
	"models": {
		"AcademicHonorsTypeGet": {
			"id": "AcademicHonorsTypeGet",
			"properties": {
				"ifNoneMatch": {
					"type": "string",
					"required": true
				}
			}
		},
		"AcademicHonorsTypePut": {
			"id": "AcademicHonorsTypePut",
			"properties": {
				"eTag": {
					"type": "string",
					"required": true
				}
			}
		},
		"AcademicHonorsTypePatch": {
			"id": "AcademicHonorsTypePatch",
			"properties": {
				"data": {
					"type": "JsonObject",
					"required": true
				},
				"ifMatch": {
					"type": "string",
					"required": true
				}
			}
		},
		"JsonObject": {
			"id": "JsonObject",
			"properties": {
				"item": {
					"type": "string",
					"required": true
				},
				"comparer": {
					"type": "IEqualityComparer`1",
					"required": true
				},
				"count": {
					"type": "int",
					"required": true
				},
				"keys": {
					"type": "KeyCollection",
					"required": true
				},
				"values": {
					"type": "ValueCollection",
					"required": true
				}
			}
		},
		"IEqualityComparer`1": {
			"id": "IEqualityComparer`1",
			"properties": {
				
			}
		},
		"KeyCollection": {
			"id": "KeyCollection",
			"properties": {
				"count": {
					"type": "int",
					"required": true
				}
			}
		},
		"ValueCollection": {
			"id": "ValueCollection",
			"properties": {
				"count": {
					"type": "int",
					"required": true
				}
			}
		},
		"AcademicHonorsTypeDelete": {
			"id": "AcademicHonorsTypeDelete",
			"properties": {
				"ifMatch": {
					"type": "string",
					"required": true
				}
			}
		},
		"AcademicHonorsTypesGet": {
			"id": "AcademicHonorsTypesGet",
			"properties": {
				"sortOrder": {
					"type": "string",
					"allowableValues": {
						"valueType": "LIST",
						"values": ["Ascending",
						"Descending"]
					},
					"required": true
				},
				"eTag": {
					"type": "string",
					"required": true
				}
			}
		},
		"AcademicHonorsTypesPost": {
			"id": "AcademicHonorsTypesPost",
			"properties": {
				"eTag": {
					"type": "string",
					"required": true
				}
			}
		}
	}
}