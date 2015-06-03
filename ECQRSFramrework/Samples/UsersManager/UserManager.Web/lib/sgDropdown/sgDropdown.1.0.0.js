angular.module('sgDropdown',[])
//Version of dialog service
.constant("sgDropdown.version",[1,0,0])
//Configuration
.service("sgDropdown.config",[function(){
	return {
		//The default dropdown template
		dropdownTemplate : "sgDialogTemplate.html",
		dropdownOpened:false
	}
}])
.run(["sgDropdown.config",'$rootScope',function(sgDropdownConfig,$rootScope){
	//loosely based on https://www.codementor.io/angularjs/tutorial/create-dropdown-control
	//Handle the click on the rest of document to close the dropdown
	angular.element(document).on("click", function(e) {
		if(!sgDropdownConfig.dropdownOpened) return;
		if(e.target.className.indexOf('inDropdown')>0) return;
		$rootScope.$broadcast("documentClicked", angular.element(e.target));
	});
}])
.directive("sgDropdown",['$rootScope','$http','sgDropdown.config','$templateCache','$compile',
		function($rootScope,$http,sgDropdownConfig,$templateCache,$compile) {
	
	return {
		restrict: "E",
		//Create an isolated scope
		scope:{
			//When nothing is selected
			placeholder: "@sgPlaceholder",
			//Items to show
			list: "=sgList",
			//Allow null
			nullable: "=?sgNullable",
			//Optional key field
			key: "@?sgKey",
			//Optinal value field
			value: "@?sgValue",
			//To call on selecton changed
			selChange:"&?sgChange"
		},
		require: 'ngModel',
		link: function(scope,element, attrs,model) {
			scope.selectedItem = undefined;
			scope.selectedValue = undefined;
			scope.listVisible = false;
			scope.isPlaceholder = true;
			
			
			//Initialize the required attribute
			var required = false;
			if(attrs.required==""){
				required = true;
			}
			var nullable = false;
			if(scope.nullable){
				nullable = scope.nullable;
			}
			
			// If key is specified even value must be specified and vice-versa
			if( (scope.key && !scope.value) || (!scope.key && scope.value) ){
				throw "sg-key and sg-value must be both/none declared! in sg-dropdown element!";
			}
			
			//When selection 
			scope.select = function(item) {
				scope.listVisible = false;
				var newValue = undefined;
				if(item){
					newValue = scope.key?item[scope.key]:item;
				}
				model.$setViewValue(newValue);
			};

			//Verify that the item is selected
			scope.isSelected = function(item) {
				if(scope.key) return item[scope.key] === scope.selectedItem[scope.key];
				return item === scope.selectedItem;
			};

			//Show the dropdown list
			scope.show = function() {
				scope.listVisible = !scope.listVisible;
				sgDropdownConfig.dropdownOpened = scope.listVisible;
			};

			//Clicking anywhere out of the item will close the dropdown
			$rootScope.$on("documentClicked", function(inner, target) {
				if(!scope.listVisible) return;
				scope.listVisible = false;
				if (!scope.$$phase) {
					scope.$apply();
				}
			});
			
			//Watcher function for the selected value
			var selectValue = function(newValue,prevValue) {
				var matching = undefined;
				//Set the model direty
				if(newValue!=prevValue){
					model.$dirty=true;
				}
				console
				//Set the validation option
				if(required){
					if(!newValue && !nullable){
						model.$setValidity('required',false);
					}else{
						model.$setValidity('required',true);
					}
				}
				
				//If no list is defined no other operations are needed
				if(!scope.list) return;
				
				//Find matching element
				for(var i=0;i<scope.list.length;i++){
					var item = scope.list[i];
					if(scope.key){
						if(item[scope.key]==newValue){
							matching = item; 
							break;
						}
					}else{
						if(item == newValue){
							matching = item;
							break;
						}
					}
				}
				
				//Select the items
				if(!matching){
					scope.isPlaceholder = true;
					scope.selectedItem = undefined;
					scope.selectedValue = undefined;
				}else{
					scope.isPlaceholder = false;
					scope.selectedItem = item;
					scope.selectedValue = scope.value?item[scope.value]:item;
				}
				
				//Invoke the selChange
				if(scope.selChange && attrs['sgChange']){
					scope.selChange()(scope.selectedItem);
				}
			}
			//Watcher function for the list
			var selectedList = function(){
				selectValue(model.$modelValue,undefined);
			}
			//To handle changes to the model made via the controller using the dropdown
			scope.$watch(function(){ return model.$modelValue;},selectValue);
			//When changing the list of data should reload the selected item
			scope.$watchCollection(function(){ return scope.list},selectedList);
			
			
			//Retrieve the template
			$http.get(sgDropdownConfig.dropdownTemplate, { cache: $templateCache })
				.then(function (response) {
					//Replace the html
					element.html(response.data);
					//Apply the new element to the scope
					$compile(element.contents())(scope);
				});
		}
	}
}]);