import React, { Component } from 'react'
import {
  CCard,
  CCardHeader,
  CCardBody,
  CButton,
  CPagination,
  CPaginationItem,
  CAlert,
  CSpinner,
  CContainer,
  CRow,
  CCol,
  CBadge,
} from '@coreui/react'
import { DocsLink } from 'src/components'
import { useLocation } from 'react-router-dom'
import PropTypes from 'prop-types'
import { Outlet, Link } from 'react-router-dom'
import { confirmAlert } from 'react-confirm-alert'; // Import
import 'react-confirm-alert/src/react-confirm-alert.css'; // Import css
import { useDispatch, useSelector } from 'react-redux'
import {saveToLocalStorage, loadFromLocalStorage, clearLocalStorage} from 'src/utility/common.js'
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

class ListBase extends Component {
  static displayName = ListBase.name
  static refreshIntervalId
  static articleListPage
  constructor(props) {
    super(props)
    this.state = {
      articles: [],
      sync:{},
      loading: true,
      curPage: 1,
      totalPage: 1,
      projectInfo: this.props.location.state,
      alarmVisible: false,
      alertMsg: '',
      alertColor: 'success',
    }
  }

  savePageState = () => {
    saveToLocalStorage({articles: this.state.articles, sync: this.state.sync, curPage: this.state.curPage, totalPage: this.state.totalPage})
  }

  componentDidMount() {
    this.populateArticleData(1)
    this.articleListPage = true
  }

  componentWillUnmount(){
    this.articleListPage = false
    clearTimeout( this.refreshIntervalId );
    console.log("loadScrappingStatus cleared")
  }

  static async loadScrappingStatus(ids) {
    //console.log("loadScrappingStatus")
    try {
      const requestOptions = {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' },
      }

      const response = await fetch(`${process.env.REACT_APP_SERVER_URL}article/scrap_status/${ids}`, requestOptions)
      let ret = await response.json()
      if (response.status === 200 && ret) {
        console.log(ret);
      }
    } catch (e) {
      console.log(e);
    }
  }

  gotoPrevPage() {
    this.populateArticleData(this.state.curPage - 1)
  }

  gotoNextPage() {
    this.populateArticleData(this.state.curPage + 1)
  }

  async scrapArticle(_id, title) {
    title = title.replaceAll('?', ';')
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}article/scrap/` + _id + '/' + title,
    )
    // this.setState({
    //   alarmVisible: false,
    //   alertMsg: 'Unfortunately, scrapping faild.',
    //   alertColor: 'danger',
    // })
    let ret = await response.json()
    console.log("scrapArticle", ret);
    if (response.status === 200 && ret) {
      //console.log('add success')
      // this.setState({
      //   alertMsg: 'Started to scrapping article from Article Forge successfully.',
      //   alertColor: 'success',
      // })
      toast.success('Started to scrapping article from Article Forge successfully.', {
        position: "top-right",
        autoClose: 5000,
        hideProgressBar: true,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
        theme: "colored",
        });
    }
    else
    {
      toast.error('Unfortunately, scrapping faild.', {
        position: "top-right",
        autoClose: 5000,
        hideProgressBar: true,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
        theme: "colored",
        });
    }
    // this.setState({ alarmVisible: true })
  }

  deleteArticleConfirm = (_id) => {
    confirmAlert({
      title: 'Warnning',
      message: 'Are you sure to delete this.',
      buttons: [
        {
          label: 'Yes',
          onClick: () => this.deleteArticle(_id)
        },
        {
          label: 'No',
          onClick: () => {return false;}
        }
      ]
    });
  };

  async deleteArticle(_id) {
    const requestOptions = {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        _id: _id,
      }),
    }
    fetch(`${process.env.REACT_APP_SERVER_URL}article/${_id}`, requestOptions)
      .then((res) => {
        if (res.status === 200) {
          let tmpData = [...this.state.articles]
          let idx = tmpData.findIndex((art) => art.id === _id)
          tmpData.splice(idx, 1)
          this.setState({
            articles: tmpData,
            loading: false,
            alarmVisible: false,
            curPage: this.state.curPage,
            totalPage: this.state.total,
          })
        }
      })
      .catch((err) => console.log(err))
  }

  async onBuild(_id, domain, ip, articleId) {
    console.log(articleId);
    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({}),
    }

    var s3Host = loadFromLocalStorage('s3host')
    var s3Name = s3Host.name == null ? "" : s3Host.name;
    var s3Region = s3Host.region == null ? "" : s3Host.region;
    const response = await fetch(
      (articleId.length > 0 ? `${process.env.REACT_APP_SERVER_URL}buildsync/${_id}/${domain}/${articleId}/${this.state.projectInfo.domainIp}?s3Name=${s3Name}&region=${s3Region}` 
                            : `${process.env.REACT_APP_SERVER_URL}buildsync/${_id}/${domain}/domainIp/${this.state.projectInfo.domainIp}?s3Name=${s3Name}&region=${s3Region}`),
      requestOptions,
    )
    this.setState({
      alertColor: 'danger',
      alertMsg: 'Zip file can not be create, unfortunatley.',
    })
    let ret = await response.json()
    if (response.status === 200 && ret) {
      this.setState({
        alertColor: 'success',
        alertMsg: 'Zip file was created successfully.',
      })
    }
    // this.setState({
    //   alarmVisible: true,
    // })
  }

  async onSync(_id, domain, ip) {
    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({}),
    }

    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}buildsync/sync/${_id}/${domain}/${ip}`,
      requestOptions,
    )
    // this.setState({
    //   alertColor: 'danger',
    //   alertMsg: 'Sync action failed, unfortunatley.',
    // })
    let ret = await response.json()
    if (response.status === 200 && ret) {
      // this.setState({
      //   alertColor: 'success',
      //   alertMsg: 'Sync action compeleted, successfully.',
      // })
      toast.success('Sync action compeleted, successfully.', {
        position: "top-right",
        autoClose: 5000,
        hideProgressBar: true,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
        theme: "colored",
        });
    }
    else
    {
      toast.error('Sync action failed, unfortunatley.', {
        position: "top-right",
        autoClose: 5000,
        hideProgressBar: true,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
        theme: "colored",
        });
    }
    // this.setState({
    //   alarmVisible: true,
    // })
  }

  async syncArticle(_id, domain, ip, articleId) {
    await this.onBuild(_id, domain, ip, articleId);
    await this.onSync(_id, domain, ip);
  }

  async downloadAllArticles(_id, domain, ip) {
    //window.open(`${process.env.REACT_APP_SERVER_URL}project/allDownload/${_id}/${domain}`, '_blank');
    try {
      const requestOptions = {
        method: 'GET',
        //mode: 'no-cors',
      }

      //console.log("progress status : ->");
      var s3Host = loadFromLocalStorage('s3host')
      var s3Name = s3Host.name == null ? "" : s3Host.name;
      var s3Region = s3Host.region == null ? "" : s3Host.region;
      fetch(`${process.env.REACT_APP_SERVER_URL}project/allDownload/${_id}/${domain}/${ip}?s3Name=${s3Name}&region=${s3Region}`, requestOptions).then(res => {
        return res.blob();
      }).then(blob => {
          const href = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = href;
          link.setAttribute('download', `${domain}.zip`);
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
      });
      
    } catch (e) {
      console.log(e);
    }
  }

  renderArticlesTable = (articles) => {
    let pageButtonCount = 3
    let pagination = <p></p>

    if (this.state.totalPage > 1) {
      let prevButton = (
        <CPaginationItem onClick={() => this.gotoPrevPage()}>Previous</CPaginationItem>
      )
      if (this.state.curPage <= 1) prevButton = <CPaginationItem disabled>Previous</CPaginationItem>

      let nextButton = <CPaginationItem onClick={() => this.gotoNextPage()}>Next</CPaginationItem>
      if (this.state.curPage >= this.state.totalPage)
        nextButton = <CPaginationItem disabled>Next</CPaginationItem>

      var pageNoAry = []
      var startNo = this.state.curPage - pageButtonCount
      var endNo = this.state.curPage + pageButtonCount
      if (startNo < 1) {
        startNo = 1
        endNo =
          pageButtonCount * 2 + 1 > this.state.totalPage
            ? this.state.totalPage
            : pageButtonCount * 2 + 1
      } else if (endNo > this.state.totalPage) {
        endNo = this.state.totalPage
        startNo = endNo - pageButtonCount * 2 > 1 ? endNo - pageButtonCount * 2 : 1
      }

      for (var i = startNo; i <= endNo; i++) {
        if (i < 1 || i > this.state.totalPage) continue
        pageNoAry.push(i)
      }

      const paginationItems = pageNoAry.map((number) => (
        <CPaginationItem
          key={number}
          onClick={() => this.populateArticleData(number)}
          active={number == this.state.curPage}
        >
          {number}
        </CPaginationItem>
      ))

      pagination = (
        <CPagination align="center" aria-label="Page navigation example">
          {prevButton}
          {paginationItems}
          {nextButton}
        </CPagination>
      )
    }

    return (
      <>
        <CAlert
          color={this.state.alertColor}
          dismissible
          visible={this.state.alarmVisible}
          onClose={() => this.setState({ alarmVisible: false })}
        >
          {this.state.alertMsg}
        </CAlert>
        <ToastContainer
            position="top-right"
            autoClose={5000}
            hideProgressBar={false}
            newestOnTop={false}
            closeOnClick
            rtl={false}
            pauseOnFocusLoss
            draggable
            pauseOnHover
            theme="colored"
          />
        <table className="table">
          <thead>
            <tr>
              <th>Id</th>
              <th>Title</th>
              <th>Action</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {articles.map((article) => {
              //if (article.content != null && article.content.length > 0)
              {
                return (<tr key={article.id}>
                  <td>{article.id}</td>
                  <td>{article.title}{((article.articleId == null || (article.articleId != '1234567890' && article.articleId != '55555'))
                                                                                        && (<>&nbsp;<CBadge color={
                                                            (article.content == null || article.content.length == 0) ? "info" : "success"
                                                            }>AF</CBadge></>) 
                                      || (article.articleId != null && article.articleId == '55555' && (<>&nbsp;<CBadge color={
                                                            (article.content == null || article.content.length == 0) ? "info" : "success"
                                                            }>OpenAI</CBadge></>) ))}
                            </td>
                  <td>
                    <CButton
                      type="button"
                      onClick={() => this.scrapArticle(article.id, article.title)}
                    >
                      Scrap
                    </CButton>
                    &nbsp;
                    <CButton
                      type="button"
                      onClick={() => this.syncArticle(this.state.projectInfo.projectid,
                                                  this.state.projectInfo.domainName,
                                                  this.state.projectInfo.domainIp,
                                                  article.id
                                                  )}
                    >
                      Sync
                    </CButton>
                    &nbsp;
                    <Link onClick={()=>this.savePageState()} to={`/article/view`} state={{ mode: 'VIEW', article: article, projectInfo: this.state.projectInfo }}>
                      <CButton type="button">View</CButton>
                    </Link>
                    &nbsp;
                    <CButton type="button" onClick={() => this.deleteArticleConfirm(article.id)}>
                      Delete
                    </CButton>
                  </td>
                  <td>
                    {article.articleId != null && article.articleId.length > 0 && this.state.sync[article.articleId] != null ? 
                      (<>{this.state.sync[article.articleId]} %</>) 
                      : 
                      this.state.sync[article.id] != null ? (<>{this.state.sync[article.id]} %</>) : (<></>)}
                  </td>
                </tr>)
              }
            })}
          </tbody>
        </table>
        {pagination}
      </>
    )
  }

  render() {
    let contents = this.state.loading ? (
      <p>
        <em>Loading...</em>
      </p>
    ) : (
      this.renderArticlesTable(this.state.articles)
    )
    return (
      <CCard className="mb-4">
        <CCardHeader>
          <CContainer>
            <CRow>
              <CCol className="align-self-start">All Articles</CCol>
              {this.state.projectInfo != null && this.state.projectInfo.projectid != null && (
                <>
                <CCol className="align-self-end" xs="auto">
                  <Link to={`/article/add`} state={{ projectId: this.state.projectInfo.projectid }}>
                    <CButton type="button">New Article</CButton>
                  </Link>
                  &nbsp;
                  <CButton
                    type="button"
                    onClick={() => this.syncArticle(this.state.projectInfo.projectid,
                                                    this.state.projectInfo.domainName,
                                                    this.state.projectInfo.domainIp,
                                                    ""
                                                    )}
                  >
                    All Sync
                  </CButton>
                  &nbsp;
                  <CButton
                    type="button"
                    onClick={() => this.downloadAllArticles(this.state.projectInfo.projectid,
                                                    this.state.projectInfo.domainName,
                                                    this.state.projectInfo.domainIp
                                                    )}
                  >
                    Download
                  </CButton>
                  </CCol>
                  </>)
              }
            </CRow>
          </CContainer>
        </CCardHeader>
        <CCardBody>{contents}</CCardBody>
      </CCard>
    )
  }

  async populateArticleData(pageNo) {
    var store = loadFromLocalStorage();
    if(store != null && store != undefined)
    {
      console.log(store)
      this.setState({
        articles: store.articles,
        sync: store.sync,
        curPage: store.curPage,
        totalPage: store.totalPage,
        loading: false,
      })
      clearLocalStorage()
      return
    }
    const projectId = this.state.projectInfo == null ? '' : this.state.projectInfo.projectid
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}article/` +
        (projectId != '' ? projectId + '/0/' : '') +
        pageNo +
        '/25',
    )
    const data = await response.json()
    this.setState({
      articles: data.data,
      loading: false,
      alarmVisible: false,
      curPage: data.curPage,
      totalPage: data.total,
    })

    let ids = "";
    let articleDocumentIds = "";
    this.setState({
      sync: {},
    })
    await data.data.map((item, index) => {
      if( /*item.isScrapping &&*/ item.articleId != null && item.articleId.length > 0)
      {
        if(item.progress != 100){
          if(ids.length > 0) ids += ",";
          ids += item.articleId;
        }
        else
        {
          var ret = this.state.sync
          ret[item.articleId] = item.progress
          this.setState({
            sync: ret,
          })
          console.log("article-list", ret, this.state.sync)
        }
      }
      else
      {
        if(articleDocumentIds.length > 0) articleDocumentIds += ",";
        articleDocumentIds += item.id;
      }
      console.log(ids, "<--", articleDocumentIds);
    });

    const refreshFunc = async () => {
      if(ids.length > 0 || articleDocumentIds.length > 0){
        try {
          const requestOptions = {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
          }
    
          //console.log("progress status : ->");
          const response = await fetch(`${process.env.REACT_APP_SERVER_URL}article/scrap_status/${ids};${articleDocumentIds}`, requestOptions)
          let ret = await response.json()
          if (response.status === 200 && ret) {
            var ret2 =  { ...this.state.sync, ...ret };
            //console.log(articleDocumentIds);
            //console.log(ret);
            this.setState({
              sync: ret2,
            })
            //console.log(this.state.sync);
          }
        } catch (e) {
          console.log(e);
        }
      }
      
      //console.log(">>>Hello World-->", this.articleListPage);
      if(this.articleListPage != null && this.articleListPage)
        this.refreshIntervalId = setTimeout(refreshFunc, 100);
    }
    this.refreshIntervalId = setTimeout(refreshFunc, 100);
  }
}

ListBase.propTypes = {
  location: PropTypes.any,
}

const List = (props) => {
  const location = useLocation()
  const dispatch = useDispatch()
  dispatch({ type: 'set', activeTab: 'article_list' })

  if (location.state == null && location.search.length > 0) {
    location.state = { projectid: new URLSearchParams(location.search).get('domainId'), 
    domainName: new URLSearchParams(location.search).get('domainName'), 
    domainIp: new URLSearchParams(location.search).get('domainIp') }
  }
  //console.log(location.state)
  //console.log(location.search)
  //console.log(new URLSearchParams(location.search).get('domainId'))
  return <ListBase location={location} {...props} />
}
export default List
