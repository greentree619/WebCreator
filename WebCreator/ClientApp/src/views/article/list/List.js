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
} from '@coreui/react'
import { DocsLink } from 'src/components'
import { useLocation } from 'react-router-dom'
import PropTypes from 'prop-types'
import { Outlet, Link } from 'react-router-dom'

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
    this.setState({
      alarmVisible: false,
      alertMsg: 'Unfortunately, scrapping faild.',
      alertColor: 'danger',
    })
    let ret = await response.json()
    console.log("scrapArticle", ret);
    if (response.status === 200 && ret) {
      //console.log('add success')
      this.setState({
        alertMsg: 'Started to scrapping article from Article Forge successfully.',
        alertColor: 'success',
      })
    }
    this.setState({ alarmVisible: true })
  }  

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

    const response = await fetch(
      (articleId.length > 0 ? `${process.env.REACT_APP_SERVER_URL}buildsync/${_id}/${domain}/${articleId}` 
                            : `${process.env.REACT_APP_SERVER_URL}buildsync/${_id}/${domain}`),
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
    this.setState({
      alertColor: 'danger',
      alertMsg: 'Sync action failed, unfortunatley.',
    })
    let ret = await response.json()
    if (response.status === 200 && ret) {
      this.setState({
        alertColor: 'success',
        alertMsg: 'Sync action compeleted, successfully.',
      })
    }
    this.setState({
      alarmVisible: true,
    })
  }

  async syncArticle(_id, domain, ip, articleId) {
    await this.onBuild(_id, domain, ip, articleId);
    await this.onSync(_id, domain, ip);
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
                  <td>{article.title}</td>
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
                    <Link to={`/article/view`} state={{ mode: 'VIEW', article: article }}>
                      <CButton type="button">View</CButton>
                    </Link>
                    &nbsp;
                    <CButton type="button" onClick={() => this.deleteArticle(article.id)}>
                      Delete
                    </CButton>
                  </td>
                  <td>
                    {article.articleId != null && article.articleId.length > 0 && this.state.sync[article.articleId] != null ? 
                      (<>{this.state.sync[article.articleId]} %</>) : (<></>)}
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
              <CCol className="align-self-end" xs="auto">
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
              </CCol>
            </CRow>
          </CContainer>
        </CCardHeader>
        <CCardBody>{contents}</CCardBody>
      </CCard>
    )
  }

  async populateArticleData(pageNo) {
    const projectId = this.state.projectInfo == null ? '' : this.state.projectInfo.projectid
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}article/` +
        (projectId != '' ? projectId + '/' : '') +
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
    await data.data.map((item, index) => {
      if( /*item.isScrapping &&*/ item.articleId != null && item.articleId.length > 0)
      {
        if(ids.length > 0) ids += ",";
        ids += item.articleId;
      }
      console.log(ids, "<--", this.state.articleIds);
    });

    const refreshFunc = async () => {
      if(ids.length > 0){
        try {
          const requestOptions = {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
          }
    
          const response = await fetch(`${process.env.REACT_APP_SERVER_URL}article/scrap_status/${ids}`, requestOptions)
          let ret = await response.json()
          if (response.status === 200 && ret) {
            var ret2 =  { ...this.state.sync, ...ret };
            //console.log("progress status : ", ret2);
            this.setState({
              sync: ret2,
            })
            console.log(this.state.sync);
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
