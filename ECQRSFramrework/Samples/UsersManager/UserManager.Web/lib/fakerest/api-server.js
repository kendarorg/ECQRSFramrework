var restServer = new FakeRest.Server();

var customers = [];

for(var i=0;i<111;i++){
	var lastName = Math.floor(i/20);
	customers.push({
		id: i, 
		first_name: 'First'+i, 
		last_name: 'Last'+lastName,
		email:"user"+1+"@test.com",
		country:i%3==0?'it':(i%3==1?'fr':'es'),
		isRetailer:i%2==0?'Y':'N',
		performance:Math.floor((Math.random() * 100) + 1)
	})
}

restServer.init({
	'customers': customers,
	'countries': [
		{id:0,code:'it',description:'Italy'},
		{id:1,code:'fr',description:'France'},
		{id:2,code:'es',description:'Spain'},
		{id:3,code:null,description:'-'}
	]
});

var doRespond = function(request,status,headers,body){	
	if (!status) status = 200;	
	return {data: body,status:status,headers: headers}
}

restServer.addCollectionInterceptor('customers',
	{
		get: function(request,id){ 
			if(parseInt(id)>9999){
				return restServer.interceptorResponse(request,500,null, {message:'Maximum id is 9999'});
			}
		},
		post: function(request,id){ 
			var body = JSON.parse(request.requestBody);
			if(body.last_name == body.first_name){
				return restServer.interceptorResponse(request,400,null, {message:'Last name must be different from first name!'});
			}
		},
		put: function(request,id){ 
			var body = JSON.parse(request.requestBody);
			if(body.last_name == body.first_name){
				console.log(body);
				return restServer.interceptorResponse(request,400,null, {message:'Last name must be different from first name!'});
			}
		}
	}
);

// use sinon.js to monkey-patch XmlHttpRequest
var server = sinon.fakeServer.create();
server.autoRespond = true;

// force the usage of filters
server.xhr.useFilters = true;

// intercept all requests, avoid the url starting with "app" that are 
// real files loaded from the hard disk, not api :)
server.xhr.addFilter(function(method, url) {
	//whenever the this returns true the request will not faked
	return (url.indexOf("app")<=1 && url.indexOf("app")>=0) || (url.indexOf("lib")<=1 && url.indexOf("lib")>=0);
});

server.respondWith(restServer.getHandler());