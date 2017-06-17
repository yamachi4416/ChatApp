angular.module('ChatApp', ['hc.marked'])
    .config(['$locationProvider', function($locationProvider) {
        $locationProvider.hashPrefix('!');
        $locationProvider.html5Mode(true);
     }]);