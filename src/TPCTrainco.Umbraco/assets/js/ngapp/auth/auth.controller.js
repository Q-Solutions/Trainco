(function() {
  'use strict';

  angular
    .module('train.auth')
    .controller('AuthbarController', AuthbarController);

  AuthbarController.$inject = ['$log', '$http', 'CONSTANTS'];
  /** @ngInject */
  function AuthbarController($log, $http, CONSTANTS) {
    var vm = this;
    vm.user = {};
    var authToken = localStorage.getItem('tcJWT');
    checkForLogin(authToken);

    function checkForLogin(authToken) {
      if (!authToken) {
        vm.isLoggedIn = false;
      } else {
          getUser(authToken);
      }
    }

    function getUser(authToken) {
      var req = {
       method: 'GET',
       url: CONSTANTS.API_ACCOUNT + '/getuser',
       headers: {
         'authorization': 'Bearer ' + localStorage.getItem('tcJWT')
       }
      }
      $http(req).then(function(response) {
        vm.user.firstName = response.data.result.firstName;
        vm.user.lastName = response.data.result.lastName;
        vm.isLoggedIn = true;
      }).catch(function(error) {
        $log.debug(error);
      })
    }

    vm.logout = function() {
      localStorage.removeItem('tcJWT');
      vm.isLoggedIn = false;
    }
  }
})();
