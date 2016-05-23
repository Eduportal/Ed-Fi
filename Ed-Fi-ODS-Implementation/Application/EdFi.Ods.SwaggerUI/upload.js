var BLOCK_SIZE = 1024 * 1024; // 1 MB

function FileViewModel(file, url) {
    var self = this;
    self.file = file;
    self.interchangeName = ko.observable();
    self.file.bytesUploaded = ko.observable(false);

    self.isValid = ko.computed(function () {
        return !!self.interchangeName();
    });

    self.status = ko.computed(function () {
        if (!self.interchangeName()) return "invalid file";
        if (!self.file.bytesUploaded()) return "not uploaded";
        if (self.file.size == self.file.bytesUploaded()) {
            return "received";
        } else {
            return self.file.bytesUploaded() + " / " + self.file.size;
        }
    });

    self.getInterchangeName = function () {
        var blob = this.file.slice(0, 100);
        var reader = new FileReader();
        reader.onloadend = function (evt) {
            var patt = /<Interchange([\w]+)/m;
            var result = evt.target.result.match(patt);
            if (result && result.length > 1)
                self.interchangeName(result[1]);
            else
                self.interchangeName(null);
        };
        reader.readAsText(blob);
    };
}

function UploadViewModel(url) {
    var self = this;
    self.error = ko.observable();
    self.status = ko.observable();
    self.files = ko.observableArray([]);
    self.operationdId = ko.observable("");
    self.token = ko.observable("");

    self.doGetOperationId = function () {
        $.ajax({
            type: "POST",
            url: url + '/bulkOperations',
            dataType: 'json',
            headers: { "Authorization": "Bearer " + self.token() },
            success: function (data, textStatus, jqXHR) {
                self.operationdId(data);
            }
        });
    };

    self.doUpload = function (f) {
        f.file.bytesUploaded(0);
        self.doUploadBlock(f.file, f.interchangeName(), 0);
    };

    self.doUploadBlock = function (f, name, n) {
        var nBlocks = Math.ceil(f.size / BLOCK_SIZE);

        if (n >= nBlocks) {
            $.ajax({
                type: "PUT",
                url: url + '/upload/' + name + '/' + self.operationdId() + '/' + n,
                cache: false,
                contentType: false,
                processData: false,
                headers: { "Authorization": "Bearer " + self.token() },
                success: function (data, textStatus, jqXHR) {
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    return 0;
                }
            });
            return;
        }
        var start = n * BLOCK_SIZE;
        var end = Math.min(f.size, n * BLOCK_SIZE + BLOCK_SIZE);
        var blob = f.slice(start, end);
        var reader = new FileReader();
        reader.onloadend = function (evt) {
            if (evt.target.readyState == FileReader.DONE) {
                var myData = new FormData();
                myData.append("file", evt.target.result);
                $.ajax({
                    type: "POST",
                    url: url + '/upload/' + name + '/' + self.operationdId() + '/' + n,
                    data: myData,
                    cache: false,
                    contentType: false,
                    processData: false,
                    headers: { "Authorization": "Bearer " + self.token() },
                    success: function (data, textStatus, jqXHR) {
                        f.bytesUploaded(end);
                        self.doUploadBlock(f, name, n + 1);
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        //todo: signal an aborted upload
                        return 0;
                    }
                });
            }
        };
        reader.readAsText(blob);
    };

    self.doStartOperation = function () {
        self.error(null);
        $.ajax({
            type: "PUT",
            url: url + '/bulkOperations/' + self.operationdId(),
            dataType: 'json',
            headers: { "Authorization": "Bearer " + self.token() },
            success: function (data, textStatus, jqXHR) {
                self.files.removeAll();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                switch (jqXHR.status) {
                    //case 400:
                    //    self.error("Maximum number of applications reached. Reuse or delete an existing client application key and secret.");
                    //    break;
                    //case 406:
                    //    self.error("You must provide a name for your client application sandbox.");
                    //    break;
                    default:
                        self.error(textStatus);
                }
                return 0;
            }
        });
    };

    self.doGetStatus = function () {
        self.error(null);
        $.ajax({
            type: "GET",
            url: self.webApiUrl + '/bulkOperations/' + self.operationdId(),
            headers: { "Authorization": "Bearer " + self.token() },
            success: function (data, textStatus, jqXHR) {
                self.status(data);
            }
        });
    };

    self.doDeleteFile = function (file) {
        self.files.remove(file);
    };
}

//
//  Custom Binding for File Drag and Drop
//
ko.bindingHandlers.fileDragDrop = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        element.addEventListener('dragover', function (evt) {
            evt.stopPropagation();
            evt.preventDefault();
            evt.dataTransfer.dropEffect = 'copy';
        }, false);
        element.addEventListener('drop', function (evt) {
            evt.stopPropagation();
            evt.preventDefault();
            var files = evt.dataTransfer.files;
            var value = valueAccessor();
            for (var i = 0, f; f = files[i]; i++) {
                value.remove(function (item) {
                    return item.file.name == f.name;
                });
                if (f.type == "text/xml") {
                    var file = new FileViewModel(f, appSettings.webApi);
                    file.getInterchangeName();
                    value.push(file);
                }
            }
        }, false);
    }
};

$(function () {
    // Check for the various File API support.
    if (window.File && window.FileReader && window.FileList && window.Blob) {
        // Great success! All the File APIs are supported.
        var viewModel = new UploadViewModel(appSettings.webApi);
        ko.applyBindings(viewModel);
    } else {
        alert('The File APIs are not fully supported in this browser.');
    }
});