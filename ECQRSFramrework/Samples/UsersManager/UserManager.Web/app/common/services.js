
common.service('globalMessagesService', ['$rootScope',function($rootScope) {
	
	$rootScope.messages = [];
	
	this.showMessage = function(text,status,level){
	    status = status ? status : 200;
		if(!level){
			if(status!=200 )level="error";
			else level = "info";
		}
		if(!text){
			if(level=="error") text = "Error "+status;
			else  text = "Info "+status;
		}
		$rootScope.messages.push({
			text:text,
			level:level
		});
	}
	
	this.clearMessages = function(){
		$rootScope.messages = [];
	}
}]);


//rejection.status
//rejection.data
//rejection.config.url
//response.data
//response.config.url
common.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.interceptors.push(['$q', '$rootScope', '$location', function ($q, $rootScope, $location) {
        return {
            request: function (config) {
                //the same config / modified config / a new config needs to be returned.
                return config;
            },
            requestError: function (rejection) {
                //It has to return the rejection, simple reject call doesn't work
                return $q.reject(rejection);
            },
            response: function (response) {
                //the same response/modified/or a new one need to be returned.
                return response;
            },
            responseError: function (rejection) {
                //It has to return the rejection, simple reject call doesn't work
                if (rejection.status == 401) {
                    var data = encodeURI(JSON.stringify({
                        data: rejection.data,
                        url: rejection.url
                    }));
                    $location.url("/401").search({data:data});
                    return {};
                }
                return $q.reject(rejection);
            }
        };
    }]);
}]);
