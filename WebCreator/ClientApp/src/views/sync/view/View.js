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
import { useDispatch, useSelector } from 'react-redux'
import {saveToLocalStorage, loadFromLocalStorage, clearLocalStorage} from 'src/utility/common.js'

class ListBase extends Component {
  static displayName = ListBase.name

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
  }

  gotoPrevPage() {
    this.populateArticleData(this.state.curPage - 1)
  }

  gotoNextPage() {
    this.populateArticleData(this.state.curPage + 1)
  }

  getLink(title) {
    title = title.replace("?", "");
    title = title.replaceAll(" ", "-");

    var url = `http://${this.state.projectInfo.projectDomain}/${title}.html`;

    //http://www.traepiller123.info.s3-website.us-east-2.amazonaws.com
    if(this.state.projectInfo.domainIp == "0.0.0.0"){
      var s3Host = loadFromLocalStorage('s3host')
      var s3Name = (s3Host.name == null || s3Host.name.length == 0) ? this.state.projectInfo.projectDomain : s3Host.name;
      var s3Region = s3Host.region == null ? "us-east-2" : s3Host.region;
      url = `http://${s3Name}.s3-website.${s3Region}.amazonaws.com/${title}.html`
    }
    
    return url;
  }

  async loadSyncStatus(ids) {
    try {
      const requestOptions = {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' },
      }

      var isAWS = (this.state.projectInfo.domainIp == "0.0.0.0" ? 1 : 0)

      var s3Host = loadFromLocalStorage('s3host')
      var s3Name = s3Host.name == null ? "" : s3Host.name;
      var s3Region = s3Host.region == null ? "" : s3Host.region;
      const response = await fetch(`${process.env.REACT_APP_SERVER_URL}article/sync_status/${this.state.projectInfo.projectDomain}/${ids}/${isAWS}?s3Name=${s3Name}&region=${s3Region}`, requestOptions)
      let ret = await response.json()
      if (response.status === 200 && ret) {
        //console.log(ret);
        var ret2 =  { ...this.state.sync, ...ret };
        this.setState({
          sync: ret2,
        })
      }
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

    const openNewPage = (article) => {
      let newLink = this.getLink(article)
      //console.log('newLink', newLink)
      window.open(
        newLink,
        '_blank' // <- This is what makes it open in a new window.
      );
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
              <th>Link</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {articles.map((article) => {
              //Omitted if (article.content != null && article.content.length > 0)
              {
                return (
                  <tr key={article.id}>
                    <td>{article.id}</td>
                    <td>
                        {article.title}
                      {/* <Link to={{pathname: `/openlink`, search: '?url='+this.getLink(article.title)}}>
                        {article.title}
                      </Link> */}
                    </td>
                    <td>
                      <button disabled={article.content == null || article.content.length == 0} onClick={() => openNewPage(article.title)}>Open Link</button>
                    </td>
                    <td>
                      {this.state.sync[article.id] == null ?  <CSpinner size="sm"/> : (this.state.sync[article.id] ? "OK" : "Failed")}
                    </td>
                  </tr>
                )
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
        <CCardHeader>All Articles</CCardHeader>
        <CCardBody>{contents}</CCardBody>
      </CCard>
    )
  }

  async populateArticleData(pageNo) {
    const projectId = this.state.projectInfo == null ? '' : this.state.projectInfo.projectid
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}article/valid/` +
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
      if(ids.length > 0) ids += ",";
      ids += item.id;

      setTimeout(()=>{
        this.loadSyncStatus(item.id);
      }, 100*index);
      console.log("sync view<--", index);
    });

    //Omitted this.loadSyncStatus(ids);
  }
}

ListBase.propTypes = {
  location: PropTypes.any,
}

const List = (props) => {
  const location = useLocation()
  const dispatch = useDispatch()
  dispatch({ type: 'set', activeTab: 'sync_view' })

  if (location.state == null && location.search.length > 0) {
    location.state = { projectid: new URLSearchParams(location.search).get('domainId'), 
    projectDomain: new URLSearchParams(location.search).get('domain'),
    domainIp: new URLSearchParams(location.search).get('domainIp') }
  }
  return <ListBase location={location} {...props} />
}
export default List
