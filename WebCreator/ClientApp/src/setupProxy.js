const createProxyMiddleware = require('http-proxy-middleware');
const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:63603';

const context =  [
  "/project",
  "/project/serpapi",
  "/project/status",
  "/project/isscrapping",
  "/project/updateSchedule",
  "/project/updatePublishSchedule",
  "/project/schedule",
  "/project/publishSchedule",
  "/project/startaf",
  "/project/themeUpload",
  "/project/allDownload",
  "/article",
  "/article/scrap",
  "/article/fromid",
  "/article/sync_status",
  "/article/scrap_status",
  "/article/update_content",
  "/article/valid",
  "/article/UpdateBatchState",
  "/article/addArticlesByTitle",
  "/article/scrapAFManual",
  "/dns",
  "/dns/byname",
  "/buildsync",
  "/buildsync/sync",
  "/setting/afsetting",
];

module.exports = function(app) {
  const appProxy = createProxyMiddleware(context, {
    target: target,
    secure: false
  });

  app.use(appProxy);
};
