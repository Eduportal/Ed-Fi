edfiApp.directive('typeahead', function() {
    return {
        restrict: 'A',
        require: 'ngModel',
        scope: { onSelect: '&' },
        link: function(scope, element, atts, ngModel) {

            var leas = new Bloodhound({
                datumTokenizer: Bloodhound.tokenizers.obj.whitespace('educationOrganizationId', 'educationOrganizationName'),
                queryTokenizer: Bloodhound.tokenizers.whitespace,
                identify: function (obj) { return obj.educationOrganizationId; },
                prefetch: 'api/leas'
            });

            var schools = new Bloodhound({
                datumTokenizer: Bloodhound.tokenizers.obj.whitespace('educationOrganizationId', 'educationOrganizationName'),
                queryTokenizer: Bloodhound.tokenizers.whitespace,
                identify: function (obj) { return obj.educationOrganizationId; },
                prefetch: 'api/schools'
            });

            element.typeahead(
                { highlight: true },
                {
                    name: 'leas',
                    display: 'educationOrganizationName',
                    source: leas,
                    limit: 10,
                    templates: {
                        header: '<div class="btn-info"><strong>Local Education Agencies</strong></div>',
                        suggestion: Handlebars.compile('<span>{{educationOrganizationName}} <small class="text-muted">({{educationOrganizationId}})</small></span>')
                    }
                },
                {
                    name: 'schools',
                    display: 'educationOrganizationName',
                    source: schools,
                    templates: {
                        header: '<div class="btn-info"><strong>Schools</strong></div>',
                        suggestion: Handlebars.compile('<span>{{educationOrganizationName}} <small class="text-muted">({{educationOrganizationId}})</small></span>')
                    }
                })
                .on('typeahead:selected', function(event, data) {
                    scope.$apply(function() {
                        ngModel.$setViewValue(data);
                    });

                    if (scope.onSelect) {
                        scope.onSelect();
                        element.typeahead('val', '');
                    }
                });
        }
    }
});
