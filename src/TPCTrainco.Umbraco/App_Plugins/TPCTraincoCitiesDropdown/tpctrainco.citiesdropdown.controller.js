angular.module("umbraco").controller("TPCTrainco.CitiesDropdownController", function ($scope, $http) {
    $scope.onLoad = function () {
        $http({
            method: 'GET',
            url: '/umbraco/backoffice/TPCTrainco/Cities/GetCities/'
        }).then(function successCallback(response) {
            $scope.cities = response.data;
        }, function errorCallback(response) {
            console.log(response);
        });
    };

    $scope.onLoad();
});