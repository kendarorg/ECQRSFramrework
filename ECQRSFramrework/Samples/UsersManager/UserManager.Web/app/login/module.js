
var loginModule = angular.module('loginModule', ['commonModule', 'sgDialogService', 'ngCookies']);

loginModule.controller('loggedUserController',[
    '$scope', '$http', '$routeParams', '$location', '$controller', 'sgDialogService', '$cookies', '$rootScope',
function ($scope, $http, $routeParams, $location, $controller, sgDialogService, $cookies, $rootScope) {

    $scope.loggedUserId = '';
    $scope.loggedUser = undefined;
    var loggedUser = function (data) {
        if (data.UserName) return data.UserName;
        return data.EMail;
    };
    if ($cookies['loggedUser']) {
        console.log($cookies['loggedUser']);
        var data = JSON.parse($cookies['loggedUser']);
        $scope.loggedUserId = loggedUser(data);
        $scope.loggedUser = data;
        $rootScope.$broadcast('loginModule.userLoggedIn', data);
    }

    $scope.$on('loginModule.userLoggedIn', function (event, data) {
        $scope.loggedUserId = loggedUser(data);
        $scope.loggedUser = data;
    });

    $scope.$on('loginModule.userLoggedOff', function (event, data) {
        $scope.loggedUserId = '';
        $scope.loggedUser = null;
    });

    $scope.logOff = function () {
        $scope.loggedUserId = '';
        $scope.loggedUser = null;
        delete $cookies['loggedUser'];
        $http.get('/api/Account/LogOff')
            .success(function (data) { $location.path('/home/');})
            .error(function (data) { $location.path('/home/'); });
    };
}]);

loginModule.controller('loginController', [
    '$scope', '$http', '$routeParams', '$location', '$controller', 'sgDialogService', '$cookies', '$rootScope',
function ($scope, $http, $routeParams, $location, $controller, sgDialogService, $cookies, $rootScope) {
	    $scope.data = {
	        Password: '',
	        UserId: '',
            RememberMe: false
	    };
	    delete  $cookies['loggedUser'];

	    $scope.login = function (data) {
	        $http.post('/api/Account/Login', data)
                .success(function (data) {
                    $cookies['loggedUser'] = JSON.stringify(data);
                    $rootScope.$broadcast('loginModule.userLoggedIn', data);
                    $location.path('/home/');
                })
                .error(function (data, status, headers, config) {
                    delete  $cookies['loggedUser'];
                    $rootScope.$broadcast('loginModule.userLoggedOff', {});
                    sgDialogService.alert("User name or Password invalid!", "Login Failed!");
                });
	    }
	}]);

