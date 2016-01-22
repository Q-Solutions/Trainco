import gulp from 'gulp';
import gutil from 'gulp-util';
import handleErrors from '../utils/handleErrors';
import config from '../config';
import pkg from '../../package.json';
import runSequence from 'run-sequence';

gulp.task('default', function() {

  if (config.args.env === 'prod') {
    gulp.start('default:prod');
  } else if (config.args.env === 'dev') {
    gulp.start('default:dev');
  } else {
    gutil.log(gutil.colors.red('--env flag should be either dev or prod'));
  }

});

gulp.task('default:dev', function() {

  runSequence(['markup:all', 'styles:fonts', 'styles', 'scripts', 'images'], ['serve', 'watch']);

});

gulp.task('default:prod', function() {

  runSequence(['markup:all', 'styles:fonts', 'images'], 'build', function() {
    if (config.args.serve) {
      gulp.start('serve');
      gutil.log(gutil.colors.yellow('Build ready, preparing to serve...'));
    }
  });

});
