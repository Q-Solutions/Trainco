export function KeywordInputDirective() {
  'ngInject';

  let directive = {
    restrict: 'E',
      template: [
        '<div class="form-group keyword-search">',
        '<h4 class="sidebar-h4">Keyword Search:</h4>',
        '<div class="search-input" ng-keydown="searchInput.handleInput($event)">',
          '<input type="text" ng-model="searchInput.query" placeholder="Enter Keyword"></input>',
          '<a href ng-click="searchInput.resetQuery()"><p ng-show="searchInput.query.length" class="mdi-navigation-close">click</p></a>',
        '</div>',
        '</div>'
      ].join(''),
    scope: {
      'query': '=',
      'search': '&onSearch',
      'clearResults': '&onClearResults'
    },
    controller: KeywordInputController,
    controllerAs: 'vm',
    bindToController: true
  };

  return directive;
}
class KeywordInputController {
    function () {

      /**
       * Handle key input
       * @param  {object} e the event
       */
      this.handleInput = (e) => {
        if (e.keyCode === 13 && this.query) {
          this.search({query: this.query});
        }
      }

      /**
       * Reset the query
       */
      this.resetQuery = () => {
        this.query = '';
        this.clearResults();
      }

    }
}
