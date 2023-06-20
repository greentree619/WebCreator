import React, { Component } from 'react'
import {
  CCard,
  CCardHeader,
  CCardBody,
  CButton,
  CAlert,
  CPagination,
  CPaginationItem,
} from '@coreui/react'
import { DocsLink } from 'src/components'
import { Outlet, Link } from 'react-router-dom'
import { ToastContainer, toast } from 'react-toastify'
import 'react-toastify/dist/ReactToastify.css'
import { alertConfirmOption } from 'src/utility/common'
import { confirmAlert } from 'react-confirm-alert'
import 'react-confirm-alert/src/react-confirm-alert.css'
import { loadFromLocalStorage, saveToLocalStorage, clearLocalStorage } from 'src/utility/common'

export default class List extends Component {
  static displayName = List.name

  constructor(props) {
    super(props)
    this.state = {
      projects: [],
      loading: true,
      alarmVisible: false,
      alertMsg: '',
      alertColor: 'success',
      curPage: 1,
      totalPage: 1,
    }
  }

  componentDidMount() {
    this.populateProjectData(1)
  }

  componentWillUnmount() 
  {//Unmount

  }

  async scrapQuery(_id, keyword, count) {
    keyword = keyword.replaceAll(';', '&')
    keyword = keyword.replaceAll('?', ';')
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}project/serpapi/` + _id + '/' + keyword + '/' + count,
    )
    // this.setState({
    //   alarmVisible: false,
    //   alertMsg: 'Unfortunately, scrapping faild.',
    //   alertColor: 'danger',
    // })
    if (response.status === 200) {
      //console.log('add success')
      // this.setState({
      //   alertMsg: 'Completed to scrapping questions from google successfully.',
      //   alertColor: 'success',
      // })
      toast.success('Completed to scrapping questions from google successfully.', alertConfirmOption);
    }
    else
    {
      toast.error('Unfortunately, scrapping faild.', alertConfirmOption);
    }
    // this.setState({ alarmVisible: true })
  }

   deleteProjectConfirm = ( _id ) => {
    confirmAlert({
      title: 'Warnning',
      message: 'Are you sure to delete this website.',
      buttons: [
        {
          label: 'Yes',
          onClick: async () => await this.delete( _id )
        },
        {
          label: 'No',
          onClick: () => {return false;}
        }
      ]
    });
  };

  async delete(_id) {
    const requestOptions = {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        _id: _id,
      }),
    }
    fetch(`${process.env.REACT_APP_SERVER_URL}project/${_id}`, requestOptions)
      .then((res) => {
        if (res.status === 200) {
          let tmpProjects = [...this.state.projects]
          let idx = tmpProjects.findIndex((pro) => pro.id === _id)
          tmpProjects.splice(idx, 1)
          this.setState({
            projects: tmpProjects,
            loading: false,
            alarmVisible: false,
            curPage: this.state.curPage,
            totalPage: this.state.total,
          })

          var allProjects = loadFromLocalStorage('allProjects')
          if(allProjects != null && allProjects != undefined)
          {
            let tmpProjects = [...allProjects]
            let idx = tmpProjects.findIndex((pro) => pro.id === _id)
            tmpProjects.splice(idx, 1)
            saveToLocalStorage(tmpProjects, 'allProjects')
          }
          toast.success('Deleted project successfully.', alertConfirmOption);
        }
        else
        {
          toast.error('Unfortunately, Deleting faild.', alertConfirmOption);
        }
      })
      .catch((err) => console.log(err))
  }

  gotoPrevPage() {
    this.populateProjectData(this.state.curPage - 1)
  }

  gotoNextPage() {
    this.populateProjectData(this.state.curPage + 1)
  }

  renderProjectsTable(state) {
    let pageButtonCount = 3
    let pagination = <p></p>

    if (this.state.totalPage > 1) {
      let prevButton = (
        <CPaginationItem onClick={() => this.gotoPrevPage()}>Previous</CPaginationItem>
      )
      if (state.curPage <= 1) prevButton = <CPaginationItem disabled>Previous</CPaginationItem>

      let nextButton = <CPaginationItem onClick={() => this.gotoNextPage()}>Next</CPaginationItem>
      if (state.curPage >= state.totalPage)
        nextButton = <CPaginationItem disabled>Next</CPaginationItem>

      var pageNoAry = []
      var startNo = state.curPage - pageButtonCount
      var endNo = state.curPage + pageButtonCount
      if (startNo < 1) {
        startNo = 1
        endNo =
          pageButtonCount * 2 + 1 > state.totalPage ? state.totalPage : pageButtonCount * 2 + 1
      } else if (endNo > state.totalPage) {
        endNo = state.totalPage
        startNo = endNo - pageButtonCount * 2 > 1 ? endNo - pageButtonCount * 2 : 1
      }

      for (var i = startNo; i <= endNo; i++) {
        if (i < 1 || i > state.totalPage) continue
        pageNoAry.push(i)
      }

      const paginationItems = pageNoAry.map((number) => (
        <CPaginationItem
          key={number}
          onClick={() => this.populateProjectData(number)}
          active={number == state.curPage}
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
          color={state.alertColor}
          dismissible
          visible={state.alarmVisible}
          onClose={() => this.setState({ alarmVisible: false })}
        >
          {state.alertMsg}
        </CAlert>
        {/* <ToastContainer
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
          /> */}
        <table className="table">
          <thead>
            <tr>
              <th className='text-center'>Id</th>
              <th className='text-center'>Domain</th>
              <th className='text-center'>Keyword</th>
              <th className='text-center'>Action</th>
            </tr>
          </thead>
          <tbody>
            {state.projects.map((project) => (
              <tr key={project.id}>
                <td className='text-center'>{project.id}</td>
                <td>
                  <Link to={`/article/list`} state={{ projectid: project.id }}>
                    {project.name}
                  </Link>
                </td>
                <td>{project.keyword}</td>
                <td className='text-center'>
                  <CButton
                    className="d-none"
                    type="button"
                    onClick={() =>
                      this.scrapQuery(project.id, project.keyword, project.quesionsCount)
                    }
                  >
                    Scrap
                  </CButton>
                  <Link to={`/project/add`} state={{ mode: 'VIEW', project: project }}>
                    <CButton type="button">View</CButton>
                  </Link>
                  &nbsp;
                  <Link to={`/project/add`} state={{ mode: 'EDIT', project: project }}>
                    <CButton type="button">Edit</CButton>
                  </Link>
                  &nbsp;
                  <CButton type="button" onClick={() => this.deleteProjectConfirm(project.id)}>
                    Delete
                  </CButton>
                </td>
              </tr>
            ))}
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
      this.renderProjectsTable(this.state)
    )
    return (
      <CCard className="mb-4">
        <CCardHeader>All Websites</CCardHeader>
        <CCardBody>{contents}</CCardBody>
      </CCard>
    )
  }

  async populateProjectData(pageNo) {
    var allProjects = loadFromLocalStorage('allProjects')
    console.log( allProjects )
    if(allProjects == null || allProjects == undefined)
    {
      const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project/` + pageNo + '/200')
      const data = await response.json()
      console.log(data.data)
      saveToLocalStorage(data.data, 'allProjects')
      this.setState({
        projects: data.data,
        loading: false,
        alarmVisible: false,
        curPage: data.curPage,
        totalPage: data.total,
      })
    }
    else
    {
      this.setState({
        projects: allProjects,
        loading: false,
        alarmVisible: false,
        curPage: 1,
        totalPage: 1,
      })
    }
  }
}
