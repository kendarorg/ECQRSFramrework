common.controller('EmptyController', [function () {

}]);

common.controller('PanesController', ['$scope', 'panes', function ($scope, panes) {
    $scope.panes = panes;
    
    $scope.selectPane = function (pane) {
        angular.forEach($scope.panes, function (forPane) {
            forPane.selected = false;
        });

        pane.selected = true;
    };
}]);

common.service('commonControllersCallbacks',[function(){
	return {
	
	}
}]);
/*
commonControllersCallbacks{
	data postLoadData:function(data,headers),
	data postGet:function(data,headers),
	data preSave:function(data),
	postSave:function(data),headers,
	data postGetUpdate:function(data,headers),
	data preUpdate:function(data),
	postUpdate:function(data,headers),
	data preDelete:function(data),
	postDelete:function(data,headers)
}*/

common.controller('ListController',['$scope','$http','globalMessagesService','sgDialogService','dataService','$routeParams','commonControllersCallbacks',
	function($scope,$http,globalMessagesService,sgDialogService,dataService,$routeParams,callbacks){
		if(dataService.keyFromRoute){
			
			var keyObject = dataService.keyFromRoute($routeParams);
			console.log(keyObject);
			if(keyObject){
				sgDialogService.openModal(
				{
					templateUrl:dataService.detailTemplate,
					data:{item:keyObject}
				});
			}
		}
		
		$scope.pageSize = 10;
		$scope.maxPages = 10;
		
		$scope.totalCount = 0;
		$scope.currentPage = 0;
		
		if($routeParams.page){
			$scope.currentPage = Math.max(parseInt($routeParams.page)-1,0);
		}	
		
		$scope.data = [];
		$scope.filter = {};
		globalMessagesService.clearMessages();
		
		$scope.search = function(){
			$scope.loadData(0);
		}

		var initializeData = function (requiredPage,data, status, headers, config)
		{
		    var listTotal = 0;

		    $scope.hasNext = data.length > $scope.pageSize;
		    if ($scope.hasNext) {
		        data = data.splice(0, $scope.pageSize);
		    }
		    //If has a count
		    if (dataService.getListCount) {
		        listTotal = dataService.getListCount(data, headers);
		    } else {
		        listTotal = $scope.pageSize * (requiredPage + 1) + ($scope.hasNext ? 1 : 0);
		    }

		    $scope.currentPage = requiredPage;

		    $scope.listTotal = listTotal;
		    if (callbacks.postLoadData) data = callbacks.postLoadData(data, headers);
		    $scope.data = data;
		}
		
		$scope.loadData = function(requiredPage){
			//Sanity check
			if(!requiredPage){
				requiredPage = 0;
			}
			
			if (dataService.getData) {
			    initializeData(requiredPage,dataService.getData(requiredPage, $scope.pageSize, $scope.pageSize + 1, $scope.filter, $scope), 200, [], null);
			} else {
			    //Getting the address
			    var address = dataService.list(requiredPage, $scope.pageSize, $scope.pageSize + 1, $scope.filter, $scope);

			    $http.get(address)
                    .success(function (data, status, headers, config) {
                        initializeData(requiredPage,data, status, headers, config);
                    })
                    .error(function (data, status, headers, config) {
                        globalMessagesService.showMessage(data.message, status);
                    });
			}
		}
		
		$scope.resetSearch = function(){
			$scope.filter = {};
			$scope.loadData(0);
		}
		
		$scope.addNew = function(){
			sgDialogService.openModal(
				{
					templateUrl:dataService.newTemplate,
					callback: function () { $scope.loadData(0); },
					data: {
					    parentScope: $scope
					}
				})
		}
		
		$scope.viewDetail = function(item) {
		    sgDialogService.openModal(
		    {
		        templateUrl: dataService.detailTemplate,
		        data: {
		            item: item,
		            parentScope: $scope
		        }
		    });
		}
		
		$scope.editDetail = function(item) {
		    sgDialogService.openModal(
		    {
		        templateUrl: dataService.editTemplate,
		        data: {
		            item: item,
		            parentScope: $scope
		        },
		        callback: function() { $scope.loadData(0); }
		    });
		}
		
		$scope.delete = function(item){
			globalMessagesService.clearMessages();
			
			sgDialogService.confirm(dataService.deleteConfirm(item),function(result){
				if(result){
					if(callbacks.preDelete){
						item = callbacks.preDelete(item);
					}
					$http.delete(dataService.delete(item,$scope)).
						success(function(data, status, headers, config) {
							if(callbacks.postDelete)callbacks.postDelete(item,headers);
							$scope.loadData();
						}).
						error(function(data,status,headers,config){
							globalMessagesService.showMessage(data.message,status);
						});
				}
			});
		}
		$scope.loadData(0);
}]);


common.controller('DetailController',['$scope','$http','$location','globalMessagesService',
		'modalInstance','dataService','commonControllersCallbacks',
	function($scope,$http,$location,globalMessagesService,
		modalInstance,dataService,callbacks){
		$scope.data = {};
		globalMessagesService.clearMessages();

		$scope.loadDetail = function () {
		    $http.get(dataService.get($scope.item, $scope)).
                success(function (data, status, headers, config) {
                    if (callbacks.postGet) {
                        data = callbacks.postGet(data, headers);
                    }
                    $scope.data = data;
                    $scope.title = dataService.detailMessage($scope.data);
                }).
                error(function (data, status, headers, config) {
                    modalInstance.dismiss();
                    globalMessagesService.showMessage(data.message, status);
                });
		}
		$scope.loadDetail();
			
		$scope.modalButtons =[
			{
				action:function(){modalInstance.dismiss();},
				text:"Ok",class:"btn-primary"
			}
		];
}]);

common.controller('NewController',['$scope','$http','$location','globalMessagesService','modalInstance','dataService','commonControllersCallbacks',
	function($scope,$http,$location,globalMessagesService,modalInstance,dataService,callbacks){
		$scope.data = {};
		globalMessagesService.clearMessages();
		
		var save = function(){
			globalMessagesService.clearMessages();
			if(callbacks.preSave){
				$scope.data = callbacks.preSave($scope.data);
			}
		    $http.post(dataService.add($scope.data, $scope), $scope.data).
				success(function(result, status, headers, config) {
					if(callbacks.postSave)callbacks.postSave($scope.data,headers);
					modalInstance.closeModal();
				}).
				error(function(data,status,headers,config){
					globalMessagesService.showMessage(data.message,status);
				});
		}
		$scope.title = dataService.createMessage();
		$scope.modalButtons =[
			{
				action:function(){modalInstance.dismiss();},
				text:"Cancel",class:"btn-default"
			},
			{
				action:function(){save();},
				text:"Save",class:"btn-primary",
				disabled: function(){if($scope.addNewForm)return $scope.addNewForm.$invalid;}
			}
		];
}]);

common.controller('EditController',['$scope','$http','$location','$routeParams','globalMessagesService','modalInstance','dataService','commonControllersCallbacks',
	function($scope,$http,$location,$routeParams,globalMessagesService,modalInstance,dataService,callbacks){
		$scope.data = {};
		globalMessagesService.clearMessages();
		
	    $http.get(dataService.get($scope.item, $scope)).
			success(function(data, status, headers, config) {
				if(callbacks.postGetUpdate){
					data = callbacks.postGetUpdate(data,headers);
				}
				$scope.data=data;
				$scope.title = dataService.editMessage($scope.data);
			}).
			error(function(data,status,headers,config){
				globalMessagesService.showMessage(data.message,status);
			});
			
		var update = function(){
			globalMessagesService.clearMessages();
			if(callbacks.preUpdate){
				$scope.data = callbacks.preUpdate($scope.data);
			}
		    $http.put(dataService.put($scope.data, $scope), $scope.data).
				success(function(result, status, headers, config) {
					if(callbacks.postUpdate)callbacks.postUpdate($scope.data,headers);
					modalInstance.closeModal();
				}).
				error(function(data,status,headers,config){
					globalMessagesService.showMessage(data.message,status);
				});
		}
		
		$scope.modalButtons =[
			{
				action:function(){modalInstance.dismiss();},
				text:"Cancel",class:"btn-default"
			},
			{
				action:function(){update();},
				text:"Update",class:"btn-primary",
				disabled: function(){
					if($scope.editForm)return $scope.editForm.$invalid || !$scope.editForm.$dirty;}
			}
		];
}]);