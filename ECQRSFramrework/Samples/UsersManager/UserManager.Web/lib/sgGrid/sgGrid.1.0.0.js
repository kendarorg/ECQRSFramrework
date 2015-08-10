angular.module('sgGrid',[])
//Version of dialog service
.constant("sgGrid.version",[1,0,0])
//Directive
.directive("sgGrid",['$http','sgGridPaginationService',
	function($http,paginationService){
	return {
	        loaded: false,
		restrict:'A',
		require: 'ngModel',
		scope:{
			sgCurrentPage:'=',
			sgPageSize:'=?',
			sgMaxPages:'=?',
			sgCount:"=",
			sgLoadData:'&',
			sgButtons:"="
		},
		link: function(scope, element, attrs,ngModel){	
			if(!scope.sgMaxPages) scope.sgMaxPages = 5;
			if(!scope.sgPageSize) scope.sgPageSize = 5;
			
			//Look for page size changes
			scope.$watch(function(){
				return scope.sgPageSize;
			},function(){
	                if (!this.loaded) {
	                    this.loaded = true;
	                    return;
	                }
	                scope.sgLoadData()(0);
	            });
			
			//Look for data changes
			scope.$watchCollection(function () {
				return ngModel.$modelValue;
			}, function() {
				var currentPage = scope.sgCurrentPage;
				
				//Find the first page to show
				var startPage = currentPage>0?Math.min(currentPage,currentPage-scope.sgMaxPages):0;
				
				//Prepare the pagination
				var paginationDescriptor = {
					currentPage:scope.sgCurrentPage,
					maxPages:scope.sgMaxPages,
					count:scope.sgCount,
					pageSize:scope.sgPageSize
				}
				
				var result = paginationService(paginationDescriptor);
				
				var newButtons=[];
				//Create the buttons
				for(var i=0;i<result.length;i++){
					var r = result[i];
					newButtons.push({
						label:r.type=="#"?r.index+1:r.type,
						pageIndex:r.index,
						selected:r.selected,
						//Here we use the this.pageIndex. If we use i, we will
						//use its reference!!!
						go: function () {
						    console.log(this.pageIndex);
						    scope.sgLoadData()(this.pageIndex);
						}
					});
				}
				//Set the buttons
				scope.sgButtons = newButtons;
		   });
		}
	}
}])
//Grid pagination service
.service('sgGridPaginationService', [function() {
/*
	var paginationDescriptor = {
		hasCount:scope.hasCount,
		currentPage:scope.currentPage,
		maxPages:scope.maxPages,
		count:listTotal,
		pageSize:scope.pageSize,
		hasNext:scope.hasNext
	}
	button {
		selected,
		index,
		type,<<,<,#,>,>>
	}
*/
	return function(pd){
		var buttons = [];
		var lastPage = 0;
		var startAt =0;
		var totalPages = -1;
		
		var createButton = function(type,index,selected){
			buttons.push({
				type:type,
				index:index,
				selected:selected?selected:false
			});
		}

		var halfMaxPages = Math.floor(pd.maxPages / 2);
		startAt = 0;
		if(pd.currentPage > halfMaxPages){
			startAt = pd.currentPage - halfMaxPages;
		}
		//console.log(startAt + " " + pd.currentPage + " " + halfMaxPages);
		totalPages = Math.ceil(pd.count/pd.pageSize);
		lastPage = Math.min(startAt+pd.maxPages,totalPages);
			
		if(startAt>0){
			createButton('<<',0);
		}
		
		if(pd.currentPage>0){
			createButton('<',pd.currentPage-1);
		}
		
		for(var i=startAt;i<lastPage;i++){
			createButton('#',i,i==pd.currentPage);
		}
		
		if(pd.currentPage<(totalPages-1)){
			createButton('>',pd.currentPage+1);
			if(pd.currentPage<(totalPages-pd.maxPages/2)){
				createButton('>>',totalPages-1);
			}
		}
		//console.log(buttons);
		return buttons;
	}
}]);