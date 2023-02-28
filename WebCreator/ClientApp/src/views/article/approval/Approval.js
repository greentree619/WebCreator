import React, { useEffect, Component } from 'react'
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
  CFormSelect,
  CFormCheck,
  CLink,
  CBadge,
  CFormInput,
} from '@coreui/react'
import { DocsLink } from 'src/components'
import { useLocation } from 'react-router-dom'
import PropTypes from 'prop-types'
import { Outlet, Link } from 'react-router-dom'
import { confirmAlert } from 'react-confirm-alert'; // Import
import 'react-confirm-alert/src/react-confirm-alert.css'; // Import css
import { useDispatch, useSelector } from 'react-redux'
import {saveToLocalStorage, loadFromLocalStorage, clearLocalStorage, alertConfirmOption, getPageFromArray } from 'src/utility/common.js'
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

class ApprovalBase extends Component {
  static displayName = ApprovalBase.name
  constructor(props) {
    super(props)
    this.state = {
      articles: [],
      sync:{},
      checkedItem:{},
      indexMap:{},
      loading: true,
      curPage: 1,
      totalPage: 1,
      projectInfo: this.props.location.state,
      alarmVisible: false,
      alertMsg: '',
      alertColor: 'success',
      articleState: 0,
      searchKeyword: '',
    }
  }

  savePageState = () => {
    saveToLocalStorage({articles: this.state.articles, sync: this.state.sync, checkedItem: this.state.checkedItem
                      , indexMap: this.state.indexMap, curPage: this.state.curPage
                      , totalPage: this.state.totalPage, articleState: this.state.articleState})
  }

  componentDidMount() {
    this.populateArticleData(1, this.state.articleState)
    console.log('child props: ', this.props.isLoadingAllArticle)
  }

  componentWillUnmount(){
  }

  componentDidUpdate(prevProps) {
    // do something
    //console.log("componentDidUpdate", prevProps, this.props)
    if( prevProps.isLoadingAllArticle && !this.props.isLoadingAllArticle )
      // || (prevProps.isLoadingAllArticle && this.props.isLoadingAllArticle) )
    {
      this.populateArticleData(1, this.state.articleState)
    }
  }

  // shouldComponentUpdate(prevProps, prevState) {
  //   if (prevState.loading !== this.state.loading
  //     || prevState.checkedItem !== this.state.checkedItem) {
  //     console.log("shouldComponentUpdate", this.state.loading)
  //     return true;
  //   }
  //   else {
  //     return false;

  //   }
  // }

  gotoPrevPage() {
    this.populateArticleData(this.state.curPage - 1, this.state.articleState)
  }

  gotoNextPage() {
    this.populateArticleData(this.state.curPage + 1, this.state.articleState)
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

  deleteBatchArticleConfirm = () => {
    confirmAlert({
      title: 'Warnning',
      message: 'Are you sure to delete selected articles.',
      buttons: [
        {
          label: 'Yes',
          onClick: () => this.setArticleState(4)
        },
        {
          label: 'No',
          onClick: () => {return false;}
        }
      ]
    });
  };

  //mode - 0: AF, 1: OpenAI  
  scrapFromAPI = async (mode) => {
    var checkedItem = this.state.checkedItem
    var articleIds = ''
    Object.keys( checkedItem ).map((item)=>{
      if( checkedItem[item].checked )
      {
        if(articleIds.length > 0) articleIds += ","
        articleIds += item
      }
    })

    const requestOptions = {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    }

    console.log(this.state.projectInfo, this.state.projectInfo.projectid);
    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}article/scrapArticleManual/${mode}/` + this.state.projectInfo.projectid + `/${articleIds}`, requestOptions)
    // this.setState({
    //   alarmVisible: false,
    //   alertMsg: 'Failed to scrapping from AF manually. Please check If AF Scheduleing is running. To use this feature must be to be off AF Scheduling',
    //   alertColor: 'danger',
    // })
    let ret = await response.json()
    if (response.status === 200 && ret) {
      //console.log('add success')
      // this.setState({
      //   alertMsg: 'Started to scrapping from AF Successfully.',
      //   alertColor: 'success',
      // })
      toast.success('Started to scrapping from '+ (mode==0 ? "AF" : "OpenAI") +' Successfully.', alertConfirmOption);
    }
    else
    {
      toast.error('Failed to scrapping from '+ (mode==0 ? "AF" : "OpenAI") +' manually. Please check If Scheduleing is running. To use this feature must be to be off Scheduling'
      , alertConfirmOption);
    }
    // this.setState({ alarmVisible: true })    
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

  async viewArticleByState(val)
  {
    //console.log("setArticleState", val)
    this.setState({
      articleState: val,
    })
    this.populateArticleData(1, val)
  }

  async setArticleState(articleState)
  {
    var checkedItem = this.state.checkedItem
    var articles = this.state.articles
    //console.log(Object.keys(checkedItem).length)
    var articleIds = ''
    Object.keys( checkedItem ).map((item)=>{
      if( checkedItem[item].checked )
      {
        articles[checkedItem[item].index].state = articleState
        if(articleIds.length > 0) articleIds += ","
        articleIds += item
      }
    })
    //console.log(articleIds)

    const requestOptions = {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
    }

    var s3Host = loadFromLocalStorage('s3host')
    var s3Name = s3Host.name == null ? "" : s3Host.name;
    var s3Region = s3Host.region == null ? "" : s3Host.region;
    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}article/UpdateBatchState/${this.state.projectInfo.projectid}/${this.state.projectInfo.domainName}/${this.state.projectInfo.domainIp}/${articleIds}/${articleState}?s3Name=${s3Name}&region=${s3Region}`, requestOptions)
    // this.setState({
    //   alarmVisible: false,
    //   alertMsg: 'Failed to change State.',
    //   alertColor: 'danger',
    //   articles: articles,
    // })
    let ret = await response.json()
    //console.log("scrapArticle", ret);
    if (response.status === 200 && ret) {
      //console.log('add success')
      // this.setState({
      //   alertMsg: 'Changed State Successfully.',
      //   alertColor: 'success',
      // })
      toast.success('Changed State Successfully.', alertConfirmOption);

      this.setState({
        articles: articles,
      })
    }
    else
    {
      toast.error('Failed to change State.', alertConfirmOption);
    }
    // this.setState({ alarmVisible: true })
  }

  onDelete()
  {
    console.log("onDelete");
  }

  getArticleState( articleState )
  {
    //console.log(articleState)
    switch(articleState)
    {
      case 0: return ("UnApproved");
      case 1: return ("UnApproved");
      case 2: return ("Approved");
      case 3: return ("Online");
    }
    return (articleState)
  }

  onChangeKeyword = (keyword) => {
    this.setState({
      searchKeyword: keyword
    })
  }

  onSearch = (keyCode) => {
    if(keyCode == 13)
    {
      //console.log("keyCode=>", keyCode)
      this.setState({
        loading: true,
      })
      this.populateArticleData(1, this.state.articleState)
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
          onClick={() => this.populateArticleData(number, this.state.articleState)}
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

    const helperBar = (id, len) => {
      if(len <= 0) return (<></>)
      else return (
        <>
          <tr>
            <td colSpan={4}>
              <table>
                <tr>
                  <td>
                    <CFormCheck id={"checkAll" + id} label="Chceck All | Selected" 
                      onChange={(e) => {
                        var checkedItem = this.state.checkedItem
                        Object.keys(this.state.checkedItem).map((item)=>{
                          checkedItem[item].checked = e.target.checked
                          //console.log(item)
                        })
                        this.setState({
                          checkedItem: checkedItem,
                        })  
                        //console.log(e.target.checked, this.state.checkedItem[article.id])
                      }}
                    />
                  </td>
                  <td className='px-2'>
                    <CButton onClick={() => this.setArticleState(2)}>Approval</CButton>
                  </td>
                  <td className='px-2'>
                    <CButton onClick={() => this.setArticleState(1)}>UnApproval</CButton>
                  </td>
                  <td className='px-2'>
                    <CButton onClick={() => this.setArticleState(3)}>Online</CButton>
                  </td>
                  <td className='px-2'>
                    <CButton onClick={() => this.scrapFromAPI(0)}>Scrap From AF</CButton>
                  </td>
                  <td className='px-2'>
                    <CButton onClick={() => this.scrapFromAPI(1)}>Scrap From OpenAI</CButton>
                  </td>
                  <td className='px-2'>
                    <CButton onClick={() => this.deleteBatchArticleConfirm()}>Delete</CButton>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
        </>
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
            {helperBar("up", articles.length)}
            {articles.map((article) => {
              //if (article.content != null && article.content.length > 0)
              {
                return (<tr key={article.id}>
                  <td><CFormCheck id={article.id} label={article.id}
                      checked={this.state.checkedItem[article.id].checked}
                      onChange={(e) => {
                        var ret = this.state.checkedItem
                        ret[article.id].checked = e.target.checked
                        this.setState({
                          checkedItem: ret,
                        })  
                        // console.log(e.target.checked, this.state.checkedItem[article.id])
                      }}/>
                  </td>
                  <td>{article.title}{((article.articleId != null && article.articleId != '1234567890' && article.articleId != '55555')
                                                                                        && (<>&nbsp;<CBadge color={
                                                            (article.content == null || article.content.length == 0) ? "info" : "success"
                                                            }>AF</CBadge></>) 
                                      || (article.articleId != null && article.articleId == '55555' && (<>&nbsp;<CBadge color={
                                                            (article.content == null || article.content.length == 0) ? "info" : "success"
                                                            }>OpenAI</CBadge></>) ))}
                  </td>
                  <td>
                    <Link onClick={()=>this.savePageState()} to={`/article/view`} state={{ mode: 'VIEW', article: article, projectInfo: this.state.projectInfo }}>
                      <CButton type="button">View</CButton>
                    </Link>
                    &nbsp;
                    <CButton type="button" onClick={() => this.deleteArticleConfirm(article.id)}>
                      Delete
                    </CButton>
                  </td>
                  <td>
                    {this.getArticleState(article.state)}
                  </td>
                </tr>)
              }
            })}
            {helperBar("down", articles.length)}
          </tbody>
        </table>
        {pagination}
      </>
    )
  }

  render() {
    let contents = this.props.isLoadingAllArticle ? (
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
              <CCol className="align-self-start">Article List</CCol>
              <CCol className="align-self-end col-5">
                <CFormInput
                  type="text"
                  id="searchKeyword"
                  aria-label="keyword"
                  value={this.state.searchKeyword}
                  onChange={(e) => this.onChangeKeyword(e.target.value)}
                  onKeyDown={(e) => this.onSearch(e.keyCode)}
                  size="sm" className="mb-3"
                />
              </CCol>
              <CCol className="align-self-end" xs="auto">
                <CFormSelect id="articleState" value={this.state.articleState} onChange={(obj) => this.viewArticleByState(obj.target.value)} size="sm" className="mb-3" aria-label="Small select example">
                  <option value="0">All Pages</option>
                  <option value="1">UnApproved</option>
                  <option value="2">Approved</option>
                  <option value="3">Online</option>
                </CFormSelect>
              </CCol>
            </CRow>
          </CContainer>
        </CCardHeader>
        <CCardBody>{contents}</CCardBody>
      </CCard>
    )
  }

  async populateArticleData(pageNo, articleState) {
    var store = loadFromLocalStorage();
    if(store != null && store != undefined)
    {
      console.log(store)
      this.setState({
        articles: store.articles,
        sync: store.sync,
        checkedItem: store.checkedItem,
        indexMap: store.indexMap,
        curPage: store.curPage,
        totalPage: store.totalPage,
        loading: false,
        articleState: store.articleState,
      })
      clearLocalStorage()
      return
    }

    this.setState({
      loading: true,
      checkedItem: {},
    })

    //{{
    // const projectId = this.state.projectInfo == null ? '' : this.state.projectInfo.projectid
    // const response = await fetch(
    //   `${process.env.REACT_APP_SERVER_URL}article/` +
    //     (projectId != '' ? projectId + '/' + articleState + '/' : '') +
    //     pageNo +
    //     '/200?keyword='+this.state.searchKeyword,
    // )
    // const data = await response.json()
    //==
    let {_data, _curPage, _total} = getPageFromArray(this.props.curProjectArticleList, 0, 200)
    //}}
    
    await _data.map((item, index) => {
      var ret = this.state.checkedItem
      ret[item.id] = {checked: false, index: index}
      this.setState({
        checkedItem: ret,
      })      
      //console.log(ids, "<--", articleDocumentIds);
    });

    this.setState({
      articles: _data,
      loading: false,
      alarmVisible: false,
      curPage: _curPage,
      totalPage: _total,
    })
  }
}

ApprovalBase.propTypes = {
  location: PropTypes.any,
  isLoadingAllArticle: PropTypes.bool,
  curProjectArticleList: PropTypes.array,
}

const Approval = (props) => {
  const location = useLocation()
  const dispatch = useDispatch()
  const curProjectArticleList= useSelector((state) => state.curProjectArticleList)
  const isLoadingAllArticle= useSelector((state) => state.isLoadingAllArticle)

  if (location.state == null && location.search.length > 0) {
    location.state = { projectid: new URLSearchParams(location.search).get('domainId'), 
    domainName: new URLSearchParams(location.search).get('domainName'), 
    domainIp: new URLSearchParams(location.search).get('domainIp') }
  }

  useEffect(() => {
    dispatch({ type: 'set', activeTab: 'article_approval' })
  }, [])
  //console.log(location.state)
  //console.log(location.search)
  //console.log(new URLSearchParams(location.search).get('domainId'))
  return <ApprovalBase location={location} isLoadingAllArticle={isLoadingAllArticle} curProjectArticleList ={curProjectArticleList} {...props} />
}
export default Approval
