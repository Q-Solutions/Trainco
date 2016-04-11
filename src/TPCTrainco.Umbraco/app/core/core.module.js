/**
 * @ngdoc overview
 * @name train.core
 * @description Core is where the additional libraries are loaded.
 */

(function(){

  'use strict';

  angular.module('train.core', [
    'ngAnimate',
    'ngSanitize',
    'ui.bootstrap',
    'selector',
    'rzModule',
    'ngStorage',
    'angular.filter',
    'darthwade.loading'
  ]).constant('CONSTANTS', {
    'API_URL': 'http://trainco-phase1.axial-client.local/api/seminars2/search/',
    'API_LIST': 'http://trainco-phase1.axial-client.local/api/seminars2/list',
    'API_SAVE': 'http://trainco-phase1.axial-client.local/api/account/UpdateSaveForLater',
    'CART_API_URL': 'http://trainco-phase1.axial-client.local/api/carts/save'});

}());
