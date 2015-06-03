var app = angular.module('app', ['ngRoute', 'sgDialogService', 'sgGrid', 'sgDropdown','sgShowError',
        'usersModule',
        'applicationsModule',
        'organizationsModule',
        'maintenanceModule'
]);

app.run(["sgDialogService.config",function(sgDialogServiceConfig){
	sgDialogServiceConfig.dialogTemplate = "lib/sgDialogService/sgDialogTemplate.html";
}]);

app.run(["sgDropdown.config",function(sgDropdownConfig){
	sgDropdownConfig.dropdownTemplate = "lib/sgDropdown/sgDropdown.html";
}]);

app.run(["sgShowError.config",function(sgShowErrorConfig){
    sgShowErrorConfig.errorTemplate = "lib/sgShowError/sgShowError.html";
}]);

app.config(['$routeProvider',
	function ($routeProvider) {
	    $routeProvider.
        when('/home/', {
            templateUrl: 'app/home.html'
        }).
        when('/401/', {
            templateUrl: 'app/common/401.html'
        }).
		/* Users part-begin */
		when('/user/', {
		    templateUrl: 'app/user/list.html'
		}).
		when('/user/:id', {
		    templateUrl: 'app/user/list.html'
		}).
	    /* Users part-end */

	    /* Applications part-begin */
	    when('/application/', {
	        templateUrl: 'app/application/list.html'
	    }).
		when('/application/:id', {
		    templateUrl: 'app/application/list.html'
		}).
	    /* Applications part-end */

	    /* Organizations part-begin */
	    when('/organization/', {
	        templateUrl: 'app/organization/list.html'
	    }).
		when('/organization/:id', {
		    templateUrl: 'app/organization/list.html'
		}).
	    /* Organizations part-end */

	    /* Maintenance part-begin */
	    when('/maintenance/', {
	        templateUrl: 'app/maintenance/home.html'
	    })
	    /* Maintenance part-end */
	    ;
	}]);

app.config(['$routeProvider',
	function($routeProvider) {
		$routeProvider.
		otherwise({
		    redirectTo: '/home/'
		});
}]);