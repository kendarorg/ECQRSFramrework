angular.module('sgShowError', [])
//Version of error service
.constant("sgShowError.version", [1, 0, 0])
//Configuration
.service("sgShowError.config", [function () {
    return {
        //The default dropdown template
        errorTemplate: "app/common/error.html",
    }
}])
.factory('sgShowError.errorTranslatorService', [
	function () {
	    var errorTranslatorService = {
	        translate: function (elementError, element) {
	            var result = new Array();
	            for (var errorKey in elementError.$error) {
	                var attributeValue = element[0].getAttribute(errorKey);
	                var error = elementError.$error[errorKey];
	                if (this.translators[errorKey] !== undefined && error) {
	                    var errorResult = this.translators[errorKey](error, attributeValue);
	                    if (errorResult != null) result.push(errorResult);
	                }
	            }
	            return result;
	        },
	        register: function (errorName, translationCallback) {
	            this.translators[errorName] = translationCallback;
	        }
	    }
	    errorTranslatorService.translators = [];

	    // default angular validators
	    errorTranslatorService.register("required", function (error) { return error ? "Value required." : null; });
	    errorTranslatorService.register("pattern", function (error) { return error ? "Not matching." : null; });
	    errorTranslatorService.register("minlength", function (error, value) { return error ? "Must be longer than " + value + "'." : null; });
	    errorTranslatorService.register("maxlength", function (error, value) { return error ? "Must be shorter than " + value + "'." : null; });
	    errorTranslatorService.register("number", function (error) { return error ? "Is not a number." : null; });
	    errorTranslatorService.register("min", function (error, value) { return error ? "Must be more than " + value + "." : null; });
	    errorTranslatorService.register("max", function (error, value) { return error ? "Must be less than " + value + "." : null; });
	    errorTranslatorService.register("url", function (error) { return error ? "Not a valid URL" : null; });
	    errorTranslatorService.register("email", function (error) { return error ? "Not a valid e-mail" : null; });
	    errorTranslatorService.register("compareTo", function (error,value) { return error ? "Value not matching" : null; });

	    return errorTranslatorService;
	}
])
.directive("sgCompareTo", [function () {
    return {
        restrict: 'A',
        require: "ngModel",
        scope: {
            otherModelValue: "=sgCompareTo"
        },
        link: function (scope, element, attributes, ngModel) {
            //The error key is "compareTo"
            ngModel.$validators.compareTo = function (modelValue) {
                return modelValue == scope.otherModelValue;
            };

            scope.$watch("otherModelValue", function () {
                ngModel.$validate();
            });
        }
    };
}])
.directive('sgShowError', ['sgShowError.errorTranslatorService', '$templateCache', 'sgShowError.config', '$http', '$compile',
	function (errorTranslatorService, $templateCache, config, $http, $compile) {

	    var getParentWithId = function (elem, id) {
	        var parent = elem;
	        while (parent[0].id.toLowerCase() != id.toLowerCase()) {
	            parent = parent.parent();
	        }
	        return parent;
	    };
	    return {
	        // restrict to an attribute.
	        restrict: 'A',
	        // require that the element has ngModel associated
	        require: 'ngModel',

	        // run to prepare all the content data
	        link: function (scope, element, attrs, model) {
	            //Will create a child scope in which will manage the errors
	            var templateScope = scope.$new();

	            //Look where should put errors
	            var showWhere = 'after';
	            var attachToElement = element;

	            if (attrs.sgAppend) {
	                var where = attrs.sgAppend.toLowerCase();
	                var splitted = where.split(':');
	                if (splitted.length == 1) {
	                    //If only one item simply search where to append
	                    showWhere = where.toLowerCase();
	                } else {
	                    //Else has an id that will show the item to append
	                    showWhere = splitted[0].trim().toLowerCase();
	                    var elementId = splitted[1].trim();
	                    var attachTo = getParentWithId(element, elementId);
	                    attachToElement = angular.element(attachTo);

	                }
	            }

	            //Retrieve the template that should use as template
	            var templateUrl = attrs.sgTemplate;
	            if (!templateUrl) {
	                //If not found take the generic oune
	                templateUrl = config.errorTemplate;
	            }

	            //Apply the styles on the item
	            templateScope.isInvalid = function () {
	                var result = model.$invalid && model.$dirty;

	                if (result) {
	                    if (attachToElement.hasClass('item-ok')) {
	                        attachToElement.removeClass('item-ok');
	                    }
	                    if (!attachToElement.hasClass('item-ko')) {
	                        attachToElement.addClass('item-ko');
	                    }
	                } else {
	                    if (attachToElement.hasClass('item-ko')) {
	                        attachToElement.removeClass('item-ko');
	                    }
	                    if (!attachToElement.hasClass('item-ok')) {
	                        attachToElement.addClass('item-ok');
	                    }
	                }
	                return result;
	            }

	            //Get the list of errors
	            templateScope.showErrors = function () {
	                if (model.$invalid && model.$dirty) {
	                    var foundErrors = errorTranslatorService.translate(model, element);
	                    return foundErrors;
	                }
	                return [];
	            }

	            //Get the list of errors as string
	            templateScope.showErrorString = function () {
	                var errors = templateScope.showErrors();
	                if (errors.length > 0) {
	                    return "Errors :" + errors.join(", ");
	                }
	                return "";
	            };

	            //Download the template from url
	            $http.get(templateUrl, { cache: $templateCache })
					.then(function (response) {
					    //Apply the new element to the child scope
					    var compiledElement = $compile(response.data)(templateScope);
					    //And attach where required
					    switch (showWhere) {
					        case ('before'):
					            attachToElement.before(compiledElement);
					            break;
					        case ('after'):
					        default:
					            attachToElement.after(compiledElement);
					            break;
					    }
					});
	        }
	    };
	}
]);